using Akka.Actor;

namespace NTPAC.LoadBalancer.Interfaces
{
  public interface IOnlineLoadBalancerActorFactory
  {
    IActorRef Create(ActorSystem actorSystem, LoadBalancerSettings settings);
  }
}
