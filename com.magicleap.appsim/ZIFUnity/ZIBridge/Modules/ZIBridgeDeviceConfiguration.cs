using ml.zi;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        public readonly DeviceConfigurationModule DeviceConfiguration = new();

        public class DeviceConfigurationModule : ModuleWrapper<DeviceConfiguration, DeviceConfigurationChanges>
        {
            public override void ConnectHandle(DeviceConfiguration handleBase)
            {
                base.ConnectHandle(handleBase);
                Load();
            }

            public void Save()
            {
                try
                {
                    Handle.Save(ProgressMonitorDisplay.Show("Saving Device Config"));
                }
                catch (ResultIsErrorException e)
                {
                    UnityEngine.Debug.LogError($"Failed to save device configuration: {e}");
                }
            }
            public void Load()
            {
                try
                {
                    Handle.Load(ProgressMonitorDisplay.Show("Loading Device Config"));
                }
                catch (ResultIsErrorException e)
                {
                    // seems like ZIF will incorrectly report this in device mode; ignore this case
                    if (e.Result != ml.zi.Result.DeviceNotFound)
                    {
                        UnityEngine.Debug.LogError($"Failed to load device configuration: {e}");
                    }
                }
            }
        }
    }
}
