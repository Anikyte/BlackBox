using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.Peripherals;

public enum FuelType
{
    
}

public static class Reactor
{ //todo: abstract core logic into Machine/Peripherals and use some kind of ID system to link API calls to "physical" devices
    //also todo: system wide integrated device system like on linux
    //for now this is a good proof of concept though
    //and the core logic is sound
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
    
    public class TemperatureDevice
    {
        public float Temperature { get; private set; }
        
        public float NeutronFlux { get; private set; }
        public float AlphaFlux { get; private set; }
        public float BetaFlux { get; private set; }
        public float GammaFlux { get; private set; }
    }

    public class Motor
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
        public float Temperature { get; internal set; }

        private readonly float jammed;
        private readonly float maxSpeed;
        private readonly float maxTorque;
        private readonly float maxTemperature;

        public Motor(Int16 jammed, float maxSpeed, float maxTorque, float maxTemperature)
        {
            this.jammed = jammed;
            this.maxSpeed = maxSpeed;
            this.maxTorque = maxTorque;
            this.maxTemperature = maxTemperature;
        }
    }

    public class RTG : TemperatureDevice, ILoop
    {

        public RTG()
        {
            
        }

        public void Loop(double deltaTime)
        {
        }
    }

    public class ControlRod : TemperatureDevice, ILoop
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

        public ControlRod(Int16 jammed)
        {
            Position = 0;

            Motor = new Motor(jammed, 16, 100, 680);
            Motor.Speed = 0;
        }

        public void Loop(double deltaTime)
        {
            position = (Int16)((Int16)Math.Floor(Motor.Speed) + position);
        }
    }

    public class Pump : TemperatureDevice, ILoop
    {
        public float FlowRate { get; private set; }

        public readonly Motor Motor;
        
        public Pump()
        {
            
        }

        public void Loop(double deltaTime)
        {
        }
    }

    public class FuelRod : TemperatureDevice, ILoop
    {
        public FuelType FuelType;

        public FuelRod()
        {
            
        }

        public void Loop(double deltaTime)
        {
        }
    }

    public static class HeatExchangerPrimary
    {
        
    }

    public static class HeatExchangerSecondary
    {
        
    }
}