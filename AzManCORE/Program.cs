using System;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzManCORE
{
    /// <summary>
    /// .NET CORE VERSION
    /// Create and manage Windows VMs in Azure using C#
    /// https://docs.microsoft.com/en-us/azure/virtual-machines/windows/csharp
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //Create the management client
            var credentials = SdkContext.AzureCredentialsFactory
                .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            //=======================================================================
            //Create resources
            //=======================================================================

            //Create the resource group
            var groupName = "myResourceGroup";
            var vmName = "myVM";
            var location = Region.USWest2;

            Console.WriteLine("Creating resource group...");
            var resourceGroup = azure.ResourceGroups.Define(groupName)
                .WithRegion(location)
                .Create();

            //Create the availability set
            Console.WriteLine("Creating availability set...");
            var availabilitySet = azure.AvailabilitySets.Define("myAVSet")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithSku(AvailabilitySetSkuTypes.Managed)
                .Create();

            //Create the public IP address
            Console.WriteLine("Creating public IP address...");
            var publicIPAddress = azure.PublicIPAddresses.Define("myPublicIP")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithDynamicIP()
                .Create();

            //Create the virtual network
            Console.WriteLine("Creating virtual network...");
            var network = azure.Networks.Define("myVNet")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("mySubnet", "10.0.0.0/24")
                .Create();

            //Create the network interface
            Console.WriteLine("Creating network interface...");
            var networkInterface = azure.NetworkInterfaces.Define("myNIC")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet("mySubnet")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                .Create();

            //Create the virtual machine
            Console.WriteLine("Creating virtual machine...");
            azure.VirtualMachines.Define(vmName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(networkInterface)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("azureuser")
                .WithAdminPassword("Azure12345678")
                .WithComputerName(vmName)
                .WithExistingAvailabilitySet(availabilitySet)
                .WithSize(VirtualMachineSizeTypes.StandardDS2V2)
                .Create();

            //To use an existing disk instead of a marketplace image
            /*
            var managedDisk = azure.Disks.Define("myosdisk")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithWindowsFromVhd("https://mystorage.blob.core.windows.net/vhds/myosdisk.vhd")
                .WithSizeInGB(128)
                .WithSku(DiskSkuTypes.PremiumLRS)
                .Create();

            azure.VirtualMachines.Define("myVM")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(networkInterface)
                .WithSpecializedOSDisk(managedDisk, OperatingSystemTypes.Windows)
                .WithExistingAvailabilitySet(availabilitySet)
                .WithSize(VirtualMachineSizeTypes.StandardDS1)
                .Create();
            */

            //Perform management tasks
            var vm = azure.VirtualMachines.GetByResourceGroup(groupName, vmName);


            //Get information about the VM
            Console.WriteLine("Getting information about the virtual machine...");
            Console.WriteLine("hardwareProfile");
            Console.WriteLine("   vmSize: " + vm.Size);
            Console.WriteLine("storageProfile");
            Console.WriteLine("  imageReference");
            Console.WriteLine("    publisher: " + vm.StorageProfile.ImageReference.Publisher);
            Console.WriteLine("    offer: " + vm.StorageProfile.ImageReference.Offer);
            Console.WriteLine("    sku: " + vm.StorageProfile.ImageReference.Sku);
            Console.WriteLine("    version: " + vm.StorageProfile.ImageReference.Version);
            Console.WriteLine("  osDisk");
            Console.WriteLine("    osType: " + vm.StorageProfile.OsDisk.OsType);
            Console.WriteLine("    name: " + vm.StorageProfile.OsDisk.Name);
            Console.WriteLine("    createOption: " + vm.StorageProfile.OsDisk.CreateOption);
            Console.WriteLine("    caching: " + vm.StorageProfile.OsDisk.Caching);
            Console.WriteLine("osProfile");
            Console.WriteLine("  computerName: " + vm.OSProfile.ComputerName);
            Console.WriteLine("  adminUsername: " + vm.OSProfile.AdminUsername);
            Console.WriteLine("  provisionVMAgent: " + vm.OSProfile.WindowsConfiguration.ProvisionVMAgent.Value);
            Console.WriteLine("  enableAutomaticUpdates: " + vm.OSProfile.WindowsConfiguration.EnableAutomaticUpdates.Value);
            Console.WriteLine("networkProfile");
            foreach (string nicId in vm.NetworkInterfaceIds)
            {
                Console.WriteLine("  networkInterface id: " + nicId);
            }
            Console.WriteLine("vmAgent");
            Console.WriteLine("  vmAgentVersion" + vm.InstanceView.VmAgent.VmAgentVersion);
            Console.WriteLine("    statuses");
            foreach (InstanceViewStatus stat in vm.InstanceView.VmAgent.Statuses)
            {
                Console.WriteLine("    code: " + stat.Code);
                Console.WriteLine("    level: " + stat.Level);
                Console.WriteLine("    displayStatus: " + stat.DisplayStatus);
                Console.WriteLine("    message: " + stat.Message);
                Console.WriteLine("    time: " + stat.Time);
            }
            Console.WriteLine("disks");
            foreach (DiskInstanceView disk in vm.InstanceView.Disks)
            {
                Console.WriteLine("  name: " + disk.Name);
                Console.WriteLine("  statuses");
                foreach (InstanceViewStatus stat in disk.Statuses)
                {
                    Console.WriteLine("    code: " + stat.Code);
                    Console.WriteLine("    level: " + stat.Level);
                    Console.WriteLine("    displayStatus: " + stat.DisplayStatus);
                    Console.WriteLine("    time: " + stat.Time);
                }
            }
            Console.WriteLine("VM general status");
            Console.WriteLine("  provisioningStatus: " + vm.ProvisioningState);
            Console.WriteLine("  id: " + vm.Id);
            Console.WriteLine("  name: " + vm.Name);
            Console.WriteLine("  type: " + vm.Type);
            Console.WriteLine("  location: " + vm.Region);
            Console.WriteLine("VM instance status");
            foreach (InstanceViewStatus stat in vm.InstanceView.Statuses)
            {
                Console.WriteLine("  code: " + stat.Code);
                Console.WriteLine("  level: " + stat.Level);
                Console.WriteLine("  displayStatus: " + stat.DisplayStatus);
            }
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();


            //Stop the VM
            Console.WriteLine("Stopping vm...");
            vm.PowerOff();
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();


            //Start the VM
            Console.WriteLine("Starting vm...");
            vm.Start();
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();


            //Resize the VM
            //Sizes for Windows virtual machines in Azure
            //https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes
            Console.WriteLine("Resizing vm...");
            vm.Update()
                .WithSize(VirtualMachineSizeTypes.StandardDS2)
                .Apply();
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();


            //Add a data disk to the VM
            Console.WriteLine("Adding data disk to vm...");
            vm.Update()
                .WithNewDataDisk(2, 0, CachingTypes.ReadWrite)
                .Apply();
            Console.WriteLine("Press enter to delete resources...");
            Console.ReadLine();


            //Tto deallocate the virtual machine
            vm.Deallocate();
            

            //Delete resources
            azure.ResourceGroups.DeleteByName(groupName);

        }
    }
}
