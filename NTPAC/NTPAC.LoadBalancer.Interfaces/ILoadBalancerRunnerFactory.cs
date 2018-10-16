namespace NTPAC.LoadBalancer.Interfaces
{
  public interface ILoadBalancerRunnerFactory
  {
    ILoadBalancerRunner CreateInstance();
  }
}
