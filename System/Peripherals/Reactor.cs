using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    
    internal static void Loop(double deltaTime)
    {
        foreach (RTG rtg in RTGs)
        {
            rtg.Loop(deltaTime);
        }

        foreach (ControlRod controlRod in ControlRods)
        {
            controlRod.Loop(deltaTime);
        }

        foreach (Pump pump in Pumps)
        {
            pump.Loop(deltaTime);
        }

        foreach (FuelRod fuelRod in FuelRods)
        {
            fuelRod.Loop(deltaTime);
        }
    } 
    
    public interface ILoop
    {
        public void Loop(double deltaTime);
    }
    
    public interface ITemperature
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
        
        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }

        private readonly float jammed;
        private readonly float maxSpeed;
        private readonly float maxTorque;
        private readonly float maxTemperature;

        internal Motor(Int16 jammed, float maxSpeed, float maxTorque, float maxTemperature) : base("motor", "motor inc", 1)
        {
            this.jammed = jammed;
            this.maxSpeed = maxSpeed;
            this.maxTorque = maxTorque;
            this.maxTemperature = maxTemperature;
        }
    }

    public class RTG : Device, ITemperature, ILoop
    {

        internal RTG() : base("rtg", "rtg inc", 2)
        {
            
        }

        public void Loop(double deltaTime)
        {
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class ControlRod : Device, ITemperature, ILoop
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

        internal ControlRod(Int16 jammed) : base("control rod", "reactor inc", 3)
        {
            Position = 0;

            Motor = new Motor(jammed, 16, 100, 680);
            Motor.Speed = 0;
        }

        public void Loop(double deltaTime)
        {
            position = (Int16)((Int16)Math.Floor(Motor.Speed) + position);
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class Pump : Device, ITemperature, ILoop
    {
        public float FlowRate { get; private set; }

        public readonly Motor Motor;

        internal Pump() : base("pump", "pump inc", 4)
        {

        }

        public void Loop(double deltaTime)
        {
        }

        public float Temperature { get; }
        public float NeutronFlux { get; }
        public float AlphaFlux { get; }
        public float BetaFlux { get; }
        public float GammaFlux { get; }
    }

    public class FuelRod : Device, ITemperature, ILoop
    {
        public FuelType FuelType;

        internal FuelRod() : base("fuel rod", "fuel inc", 5)
        {

        }

        public void Loop(double deltaTime)
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