1.移动的逻辑
I:\ZWPro\RvoProject\Assets\Scripts\GameAgent.cs
	Vector2 pos = Simulator.Instance.getAgentPosition(sid);
	transform.position = new Vector3(pos.x, transform.position.y, pos.y);
2.获取位置 getAgentPosition(int agentNo)
I:\ZWPro\RvoProject\Assets\testtt\KFrameWork\FrameWork\Modules\RVO\Source\Simulator.cs
3. 更新位置

        internal void update()
        {
            velocity_ = newVelocity_;
            position_ += velocity_ * Simulator.Instance.timeStep_;
        }

I:\ZWPro\RvoProject\Assets\testtt\KFrameWork\FrameWork\Modules\RVO\Source\Agent.cs
4.计算方向上的力
   line.direction = RVOMath.normalize((lines[j].direction - lines[i].direction));
5.计算附近的人给的力


            /* Create agent ORCA lines. */
            for (int i = 0; i < agentNeighbors_.Count; ++i)
            {
                Agent other = agentNeighbors_[i].Value;

                KInt2 relativePosition = other.position_ - position_;