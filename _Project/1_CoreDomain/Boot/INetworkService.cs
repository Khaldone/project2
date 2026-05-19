// 2. Assets/Scripts/CoreDomain/Boot/INetworkService.cs
using System.Threading.Tasks;
public interface INetworkService
{
    Task<bool> ConnectToMasterServerAsync();
}