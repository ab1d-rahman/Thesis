using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Linq;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    class Point
    {
        public double X, Z;
        public Point(double xx,double zz)
        {
            this.X = xx;
            this.Z = zz;
        }
    }

    class Worker
    {
        private const float InferredZPositionClamp = 0.1f;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;

        private int count = 0;
        private bool tracking = false;

        Point userPosition, currentBox, currentBoxCorner, start, end, robot, intersection;
        StreamWriter file;

        public Worker()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            robot = new Point(15,20);
            intersection = new Point(0, 0);
            File.WriteAllText("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt","");
            //file = new StreamWriter("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt", true);
            if(kinectSensor != null)
            {
                this.kinectSensor.Open();
            }
        }

        public bool isAbove(Point P,double m,double c)
        {
            return m*P.X-P.Z+c > 0;
        }

        public double distance(Point a, Point b)
        {
            return Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Z-b.Z)*(a.Z-b.Z));
        }

        public void GetFrames()
        {
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
        }

        private void Reader_FrameArrived(object sender,BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using(BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if(bodyFrame != null)
                {
                    if(this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if(dataReceived)
            {
                foreach(Body body in this.bodies)
                {

                    if(body.IsTracked)
                    {

                        IReadOnlyDictionary<JointType,Joint> joints = body.Joints;



                        CameraSpacePoint HandPosition = joints[JointType.HandRight].Position;
                        CameraSpacePoint ShoulderPosition = joints[JointType.ShoulderRight].Position;
                        if(HandPosition.Z < 0)
                        {
                            HandPosition.Z = InferredZPositionClamp;
                        }
                        if(ShoulderPosition.Z < 0)
                        {
                            ShoulderPosition.Z = InferredZPositionClamp;
                        }

                        start = new Point((double)ShoulderPosition.X*100.0,(double)ShoulderPosition.Z*100.0);
                        end = new Point((double)HandPosition.X*100.0,(double)HandPosition.Z*100.0);


                        //Console.WriteLine(start.X + " " + start.Z + " " + end.X + " " + end.Z);

                        // calculating slope(m) and intercept(c)
                        double dz = end.Z - start.Z;
                        double dx = end.X - start.X;

                        double m1 = dz/dx;
                        double c1 = start.Z-m1*start.X;

                        double m2 = -(1/m1);
                        double c2 = robot.Z-(robot.X*m2);

                        intersection.X = (c2-c1)/(m1-m2);
                        intersection.Z = m1*intersection.X+c1;

                        double length = 500.0;
                        double alpha = Math.Atan2(end.Z-start.Z,end.X-start.X);
                        end = new Point(start.X+length*Math.Cos(alpha),start.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position


                        File.AppendAllText("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt","s " + start.X + " " + start.Z + Environment.NewLine);
                        File.AppendAllText("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt", "p " + end.X + " " + end.Z + Environment.NewLine);

                        File.AppendAllText("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt","i " + intersection.X + " " + intersection.Z + Environment.NewLine);

                        Console.WriteLine("p " + end.X + " " + end.Z);

                        if(body.HandLeftState == HandState.Open)
                        {
                            if(distance(intersection, robot) > 10.0)
                            {
                                length = 5.0;
                                alpha = Math.Atan2(intersection.Z-robot.Z, intersection.X-robot.X);
                                robot = new Point(robot.X+length*Math.Cos(alpha), robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position
                                                   
                            }
                            else
                            {
                                length = 10.0;
                                alpha = Math.Atan2(end.Z-robot.Z, end.X-robot.X);
                                robot = new Point(robot.X+length*Math.Cos(alpha), robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position
                               
                            }

                            File.AppendAllText("D:\\Thesis\\Codes\\Thesis-Git\\Simulator\\in.txt","r " + robot.X + " " + robot.Z + Environment.NewLine);
                            Console.WriteLine("r " + robot.X + " " + robot.Z);

                        }

                        System.Threading.Thread.Sleep(200);

                    }
                }
            }
        }
    }


    class Program
    {
        public static Worker worker;
        static void Main(string[] args)
        {
            worker = new Worker();
            worker.GetFrames();

            while(true) ;
        }
    }
}
