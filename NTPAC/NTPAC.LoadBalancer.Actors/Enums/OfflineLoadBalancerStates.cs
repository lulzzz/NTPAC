namespace NTPAC.LoadBalancer.Actors.Enums
{
  public enum OfflineLoadBalancerStates
  {
    WaitingForProcessingRequest,
    LoadingBatch,
    SendingBatch,
    Finalizing,
    Finalized
  }
}
