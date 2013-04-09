namespace Fubu.Running
{
    public class StartApplication
    {
        public string ApplicationName { get; set; }
        public int PortNumber { get; set; }
        public string PhysicalPath { get; set; }

        public bool UseProductionMode { get; set; }

        public override string ToString()
        {
            return string.Format("ApplicationName: {0}, PortNumber: {1}, PhysicalPath: {2}", ApplicationName, PortNumber, PhysicalPath);
        }
    }
}