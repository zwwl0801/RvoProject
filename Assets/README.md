1.�ƶ����߼�
I:\ZWPro\RvoProject\Assets\Scripts\GameAgent.cs
	Vector2 pos = Simulator.Instance.getAgentPosition(sid);
	transform.position = new Vector3(pos.x, transform.position.y, pos.y);
2.��ȡλ�� getAgentPosition(int agentNo)
I:\ZWPro\RvoProject\Assets\testtt\KFrameWork\FrameWork\Modules\RVO\Source\Simulator.cs
3. ����λ��

        internal void update()
        {
            velocity_ = newVelocity_;
            position_ += velocity_ * Simulator.Instance.timeStep_;
        }

I:\ZWPro\RvoProject\Assets\testtt\KFrameWork\FrameWork\Modules\RVO\Source\Agent.cs
4.���㷽���ϵ���
   line.direction = RVOMath.normalize((lines[j].direction - lines[i].direction));
5.���㸽�����˸�����


            /* Create agent ORCA lines. */
            for (int i = 0; i < agentNeighbors_.Count; ++i)
            {
                Agent other = agentNeighbors_[i].Value;

                KInt2 relativePosition = other.position_ - position_;