using Akka.Actor;

namespace NTPAC.LoadBalancer.Interfaces
{
  public interface IOfflineLoadBalancerActorFactory
  {
    IActorRef Create(ActorSystem actorSystem, LoadBalancerSettings settings);
  }
}
