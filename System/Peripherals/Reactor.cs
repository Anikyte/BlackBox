namespace System.Peripherals;

public enum FuelType
{
    
}

public static class Reactor
{ 
    public static List<RTG> RTGs { get; } = new List<RTG>();
    public static List<ControlRod> ControlRods { get; } = new List<ControlRod>();
    public static List<Pump> Pumps { get; } = new List<Pump>();
    public static List<FuelRod> FuelRods { get; } = new List<FuelRod>();

    internal static void Initialize(Random random, int rtgs, int controlRods, int pumps, int fuel)
    {
        for (var i = 0; i < rtgs; i++)
        {
            RTGs.Add(new RTG());
        }

        for (var i = 0; i < controlRods; i++)
        {
            ControlRods.Add(new ControlRod((short)random.Next(0, short.MaxValue)));
        }

        for (var i = 0; i < pumps; i++)
        {
            Pumps.Add(new Pump());
        }

        for (var i = 0; i < fuel; i++)
        {
            FuelRods.Add(new FuelRod());
        }
    }
    
    interface ITemperature
    {
        public float Temperature { get; }
        
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class Motor : Device, ITemperature
    {
        public float Speed
        {
            get { return 0; }
            set
            {
                Math.Clamp(value, -maxSpeed, maxSpeed);
            }
        }

        public float Torque { get; private set; } //todo: function of speed
        public float BackEMF { get; internal set; }

        float ITemperature.Temperature { get; }
        float ITemperature.NeutronFlux { get; }
        float ITemperature.AlphaFlux { get; }
        float ITemperature.BetaFlux { get; }
        float ITemperature.GammaFlux { get; }

        private readonly float jammed;
        private readonly float maxSpeed;
        private readonly float maxTorque;
        private readonly float maxTemperature;

        internal Motor(Int16 jammed, float maxSpeed, float maxTorque, float maxTemperature) : base("motor", "motor inc", 0x01)
        {
            this.jammed = jammed;
            this.maxSpeed = maxSpeed;
            this.maxTorque = maxTorque;
            this.maxTemperature = maxTemperature;
        }
    }

    public class RTG : Device, ITemperature
    {

        internal RTG() : base("rtg", "rtg inc", 0x02)
        {
            
        }

        internal override void Loop(double deltaTime)
        {
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class ControlRod : Device, ITemperature
    {
        private Int16 targetPosition = 0;
        private Int16 position = 0;

        public Int16 Position //consider using regular int as int16 are implicitly cast to int32 when doing operations(??)
        {
            get { return position; }
            set
            {
                targetPosition = value;
            }
        }

        public readonly Motor Motor;

        internal ControlRod(Int16 jammed) : base("control rod", "reactor inc", 0x03)
        {
            Position = 0;

            Motor = new Motor(jammed, 16, 100, 680);
            Motor.Speed = 0;
        }

        internal override void Loop(double deltaTime)
        {
            position = (Int16)((Int16)Math.Floor(Motor.Speed) + position);
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class Pump : Device, ITemperature
    {
        public float FlowRate { get; private set; }

        public readonly Motor Motor;

        internal Pump() : base("pump", "pump inc", 0x04)
        {

        }

        internal override void Loop(double deltaTime)
        {
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class FuelRod : Device, ITemperature
    {
        public FuelType FuelType;

        internal FuelRod() : base("fuel rod", "fuel inc", 0x05)
        {

        }

        internal override void Loop(double deltaTime)
        {
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public static class HeatExchangerPrimary
    {
        
    }

    public static class HeatExchangerSecondary
    {
        
    }
}