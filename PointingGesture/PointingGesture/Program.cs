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
        public Point(double xx, double zz)
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
        
        Point userPosition, currentBox, currentBoxCorner, start, end;

        public Worker()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            if(kinectSensor != null)
            {
                this.kinectSensor.Open();
            }
        }

        public bool isAbove(Point P, double m, double c)
        {
            return m*P.X-P.Z+c > 0;
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

                        start = new Point((double)ShoulderPosition.X*100.0, (double)ShoulderPosition.Z*100.0);
                        end = new Point((double)HandPosition.X*100.0,(double)HandPosition.Z*100.0);

                  
                        //Console.WriteLine(start.X + " " + start.Z + " " + end.X + " " + end.Z);

                        double dz = end.Z - start.Z;
                        double dx = end.X - start.X;

                        double m = dz/dx;

                        double c = start.Z-m*start.X;

                        double length = 500.0;
                        double alpha = Math.Atan2(end.Z-start.Z, end.X-start.X);
                        end = new Point(start.X+length*Math.Cos(alpha), start.Z+length*Math.Sin(alpha));

                        if(start.X < 0.0) userPosition = new Point((start.X-50.0)/50.0,start.Z/50.0);
                        else userPosition = new Point(start.X/50.0,start.Z/50.0);
                        currentBox = new Point((int)userPosition.X,(int)userPosition.Z);

                        //Console.WriteLine("new -->" + start.X + " " + start.Z + " " + end.X + " " + end.Z);
                        //Console.WriteLine("mc " + m + " " + c);

                        Console.WriteLine("You are standing on: " + currentBox.X + ", " + currentBox.Z);
                        Console.WriteLine("");
                        Console.WriteLine("You are pointing to:");
                        currentBoxCorner = new Point(0.0, 0.0);
                        if(dx > 0.0)
                        {
                            //currentBoxCorner = new Point((currentBox.X+1.0)*50.0,(currentBox.Z)*50.0);
                            currentBoxCorner.X = (currentBox.X+1.0)*50.0;
                            currentBoxCorner.Z = currentBox.Z*50.0;

                            while(currentBoxCorner.X < end.X && currentBoxCorner.Z > end.Z)
                            {
                                Console.WriteLine(currentBox.X + " " + currentBox.Z);
                                if(isAbove(currentBoxCorner,m,c))
                                {
                                    currentBox.X += 1;
                                }
                                else currentBox.Z -= 1;

                                currentBoxCorner.X = (currentBox.X+1.0)*50.0;
                                currentBoxCorner.Z = currentBox.Z*50.0;
                            }
                        }                        
                        else
                        {
                            //currentBoxCorner = new Point((currentBox.X)*50.0,(currentBox.Z)*50.0);
                            currentBoxCorner.X = currentBox.X*50.0;
                            currentBoxCorner.Z = currentBox.Z*50.0;

                            while(currentBoxCorner.X > end.X && currentBoxCorner.Z > end.Z)
                            {
                                Console.WriteLine(currentBox.X + " " + currentBox.Z);
                                if(isAbove(currentBoxCorner,m,c))
                                {
                                    currentBox.X -= 1;
                                }
                                else currentBox.Z -= 1;

                                currentBoxCorner.X = currentBox.X*50.0;
                                currentBoxCorner.Z = currentBox.Z*50.0;
                            }
                        }

                        Console.WriteLine("");
                        System.Threading.Thread.Sleep(1000);

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
