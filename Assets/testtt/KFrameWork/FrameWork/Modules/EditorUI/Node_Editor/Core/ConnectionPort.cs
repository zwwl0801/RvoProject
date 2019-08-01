﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using NodeEditorFramework.Utilities;

namespace NodeEditorFramework 
{
	public enum NodeSide { Left = 4, Top = 3, Right = 2, Bottom = 1 }
	public enum Direction { None, In, Out }
	public enum ConnectionShape { Line, Bezier }
	public enum ConnectionCount { Single, Multi, Max }

	[System.Serializable]
	public class ConnectionPort : ScriptableObject
	{
		// Properties
		public Node body;
		public Direction direction = Direction.None;
		public ConnectionCount maxConnectionCount = ConnectionCount.Multi;
		public virtual ConnectionShape shape { get { return ConnectionShape.Line; } }

		// Connection Style
		public string styleID;
		protected ConnectionPortStyle _connectionStyle;
		protected ConnectionPortStyle ConnectionStyle { get { CheckConnectionStyle (); return _connectionStyle; } }
		protected virtual Type styleBaseClass { get { return typeof(ConnectionPortStyle); } }
		[NonSerialized]
		public Color color = Color.white;

		// Connections
		[SerializeField]
		protected List<ConnectionPort> _connections = new List<ConnectionPort> ();
		public List<ConnectionPort> connections { get { return _connections; } }

		public void Init (Node nodeBody, string knobName) 
		{
			body = nodeBody;
			name = knobName;
		}

		public void Validate(Node nodeBody)
		{
			if (body == null)
			{
				LogMgr.LogError("Port " + name + " has no node body assigned! Fixed!");
				body = nodeBody;
			}
			if (_connections == null)
				_connections = new List<ConnectionPort>();
			int originalCount = _connections.Count;
			_connections = _connections.Where(o => o != null).ToList();
			if (originalCount != _connections.Count)
				LogMgr.LogWarning("Removed " + (originalCount - _connections.Count) + " broken (null) connections from node " + body.name + "! Automatically fixed!");
		}

		public virtual IEnumerable<string> AdditionalDynamicKnobData()
		{
			return new List<string>() { "styleID", "direction", "maxConnectionCount" };
		}

		#region Connection GUI

		/// <summary>
		/// Checks and fetches the connection style declaration specified by the styleID
		/// </summary>
		protected void CheckConnectionStyle ()
		{
			if (_connectionStyle == null || !_connectionStyle.isValid ()) 
			{
				_connectionStyle = ConnectionPortStyles.GetPortStyle (styleID, styleBaseClass);
				if (_connectionStyle == null || !_connectionStyle.isValid())
				{ // Generate consistent color for a type - using string because it delivers greater variety of colors than type hashcode
#if UNITY_5_4_OR_NEWER
                    UnityEngine.Random.InitState(styleID.GetHashCode());
#endif
                    color = UnityEngine.Random.ColorHSV(0, 1, 0.6f, 0.8f, 0.8f, 1.4f);
				}
				else
					color = _connectionStyle.Color;
			}
		}

		/// <summary>
		/// Draws the connection lines from this port to all it's connections
		/// </summary>
		public virtual void DrawConnections () 
		{
			if (Event.current.type != EventType.Repaint)
				return;
			Vector2 startPos = body.rect.center;
			foreach (ConnectionPort connectionPort in connections)
			{
				LogMgr.Log ("Drawing connection from " + name + " to " + connectionPort.name);
				Vector2 endPos = connectionPort.body.rect.center;
				NodeEditorGUI.DrawConnection (startPos, endPos, ConnectionDrawMethod.StraightLine, color);
			}
		}

		/// <summary>
		/// Draws a connection line from the current knob to the specified position
		/// </summary>
		public virtual void DrawConnection (Vector2 endPos) 
		{
			if (Event.current.type != EventType.Repaint)
				return;
			NodeEditorGUI.DrawConnection (body.rect.center, endPos, ConnectionDrawMethod.StraightLine, color);
		}

#endregion

#region Connection Utility

		/// <summary>
		/// Returns whether this port is connected to any other port
		/// </summary>
		public bool connected () 
		{
			return connections.Count > 0;
		}

		/// <summary>
		/// Returns the connection with the specified index or null if not existant
		/// </summary>
		public ConnectionPort connection (int index) 
		{
			if (connections.Count <= index)
				throw new IndexOutOfRangeException ("connections[" + index + "] of '" + name + "'");
			return connections[index];
		}

		/// <summary>
		/// Tries to apply a connection between this port and the specified port
		/// </summary>
		public bool TryApplyConnection (ConnectionPort port, bool silent = false)
		{
			if (CanApplyConnection (port)) 
			{ // This port and the specified port can be connected
				ApplyConnection (port, silent);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether a connection can be applied between this port and the specified port
		/// </summary>
		public virtual bool CanApplyConnection (ConnectionPort port)
		{
			if (port == null || body == port.body || connections.Contains (port))
				return false;

			if (direction == Direction.None && port.direction == Direction.None)
				return true; // None-Directive connections can always connect

			if (direction == Direction.In && port.direction != Direction.Out)
				return false; // Cannot connect inputs with anything other than outputs
			if (direction == Direction.Out && port.direction != Direction.In)
				return false; // Cannot connect outputs with anything other than inputs
			
			if (direction == Direction.Out) // Let inputs handle checks for recursion
				return port.CanApplyConnection (this);

			if (port.body.isChildOf (body))
			{ // Recursive
				if (!port.body.allowsLoopRecursion (body))
				{
					// TODO: Generic Notification
					LogMgr.LogWarning ("Cannot apply connection: Recursion detected!");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Applies a connection between between this port and the specified port. 
		/// 'CanApplyConnection' has to be checked before to avoid interferences!
		/// </summary>
		public void ApplyConnection (ConnectionPort port, bool silent = false)
		{
			if (port == null) 
				return;

			if (maxConnectionCount == ConnectionCount.Single && connections.Count > 0)
			{ // Respect maximum connection count on this port
				connections[0].connections.Remove (this);
				connections.Clear ();
			}
			connections.Add(port);

			if (port.maxConnectionCount == ConnectionCount.Single && port.connections.Count > 0)
			{ // Respect maximum connection count on the other port
				port.connections[0].connections.Remove (port);
				port.connections.Clear ();
			}
			port.connections.Add (this);

			if (!silent)
			{ // Callbacks
				port.body.OnAddConnection (port, this);
				body.OnAddConnection (this, port);
				NodeEditorCallbacks.IssueOnAddConnection (this, port);
				NodeEditor.curNodeCanvas.OnNodeChange(direction == Direction.In? port.body : body);
			}
		}

		/// <summary>
		/// Clears all connections of this port to other ports
		/// </summary>
		public void ClearConnections (bool silent = false)
		{
			while (connections.Count > 0)
				RemoveConnection (connections[0], silent);
		}

		/// <summary>
		/// Removes the connection of this port to the specified port if existant
		/// </summary>
		public void RemoveConnection (ConnectionPort port, bool silent = false)
		{
			if (port == null)
			{
				connections.RemoveAll (p => p != null);
				return;
			}

			if (!silent) NodeEditorCallbacks.IssueOnRemoveConnection (this, port);
			port.connections.Remove (this);
			connections.Remove (port);

			if (!silent) NodeEditor.curNodeCanvas.OnNodeChange (body);
		}

#endregion
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ConnectionPortAttribute : Attribute
	{
		public string Name;
		public string StyleID;
		public Direction Direction;
		public ConnectionCount MaxConnectionCount;

		public virtual Type ConnectionType { get { return typeof(ConnectionPort); } }

		public ConnectionPortAttribute(string name)
		{
			Setup (name, Direction.None, "Default", ConnectionCount.Multi);
		}
		public ConnectionPortAttribute(string name, Direction direction)
		{
			Setup (name, direction, "Default", ConnectionCount.Multi);
		}
		public ConnectionPortAttribute(string name, Direction direction, ConnectionCount maxCount)
		{
			Setup (name, direction, "Default", maxCount);
		}
		public ConnectionPortAttribute(string name, string styleID)
		{
			Setup (name, Direction.None, styleID, ConnectionCount.Multi);
		}
		public ConnectionPortAttribute(string name, Direction direction, string styleID)
		{
			Setup (name, direction, styleID, ConnectionCount.Multi);
		}
		public ConnectionPortAttribute(string name, Direction direction, string styleID, ConnectionCount maxCount)
		{
			Setup (name, direction, styleID, maxCount);
		}

		private void Setup (string name, Direction direction, string styleID, ConnectionCount maxCount) 
		{
			Name = name;
			Direction = direction;
			StyleID = styleID;
			MaxConnectionCount = maxCount;
		}

		public bool MatchFieldType (Type fieldType) 
		{
			return fieldType.IsAssignableFrom (ConnectionType);
		}

		public virtual bool IsCompatibleWith (ConnectionPort port)
		{
			if (port.GetType() != ConnectionType)
				return false;
			if (port.styleID != StyleID)
				return false;
			if (!(Direction == Direction.None && port.direction == Direction.None)
				&& !(Direction == Direction.In && port.direction == Direction.Out)
				&& !(Direction == Direction.Out && port.direction == Direction.In))
				return false;
			return true;
		}

		public virtual ConnectionPort CreateNew (Node body) 
		{
			ConnectionPort port = ScriptableObject.CreateInstance<ConnectionPort> ();
			port.Init (body, Name);
			port.direction = Direction;
			port.styleID = StyleID;
			port.maxConnectionCount = MaxConnectionCount;
			return port;
		}

		public virtual void UpdateProperties (ConnectionPort port) 
		{
			port.name = Name;
			port.direction = Direction;
			port.styleID = StyleID;
			port.maxConnectionCount = MaxConnectionCount;
		}
	}

	[ReflectionUtility.ReflectionSearchIgnoreAttribute ()]
	public class ConnectionPortStyle
	{
		protected string identifier;
		protected Color color;

		public virtual string Identifier { get { return identifier; } }
		public virtual Color Color { get { return color; } }

		public ConnectionPortStyle () {}

		public ConnectionPortStyle (string name) 
		{
			identifier = name;
			GenerateColor();
		}

		public void GenerateColor ()
		{
            // Generate consistent color for a type - using string because it delivers greater variety of colors than type hashcode

#if UNITY_5_4_OR_NEWER
            int srcInt = (int)(Identifier.GetHashCode());
			UnityEngine.Random.InitState (srcInt);
#endif
            color = UnityEngine.Random.ColorHSV (0, 1, 0.6f, 0.8f, 0.8f, 1.4f);
		}

		public virtual bool isValid () 
		{
			return true;
		}
	}
}
