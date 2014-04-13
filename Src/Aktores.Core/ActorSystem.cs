﻿namespace Aktores.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ActorSystem : ActorRefFactory
    {
        private TaskQueue queue = new TaskQueue();

        public ActorSystem()
        {
            for (int k = 0; k < 10; k++)
                (new Worker(this.queue)).Start();
        }

        public override ActorRef ActorOf(Type t, string name = null)
        {
            var actor = (Actor)Activator.CreateInstance(t);
            return this.ActorOf(actor, name);
        }

        public override ActorRef ActorOf(Actor actor, string name = null)
        {
            var actorref = new ActorRef(this, actor, new Mailbox());

            if (string.IsNullOrWhiteSpace(name))
                name = Guid.NewGuid().ToString();

            this.Register(actorref, name);

            actor.Self = actorref;
            actor.Context = this;

            actor.Initialize();

            lock (actor)
                actor.Start();

            return actorref;
        }

        public override void Stop(ActorRef actorref)
        {
            actorref.Actor.Stop();
        }

        public void Shutdown()
        {
            for (int k = 0; k < 10; k++)
                this.queue.Add(null);

            foreach (var actorref in this.ActorRefs)
                this.Stop(actorref);
        }

        internal void AddTask(ActorTask task)
        {
            this.queue.Add(task);
        }
    }
}
