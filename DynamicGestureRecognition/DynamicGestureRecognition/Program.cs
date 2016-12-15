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
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using System.Linq;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    public class Worker
    {
        private const float InferredZPositionClamp = 0.1f;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;

        private int count = 0;
        private bool tracking = false;

        double prevx, prevy, prevz, currTime, prevTime, x, y, z, speedthreshold = 120.0;
        Stopwatch stopwatch;

        bool isTraining, onTraining;
        string currentGestureName;
        List<int> FeatureVector;
        List<List<int>> TrainingSequence;
        int numberOfTrainingSequences;
        int[][] sequences;
        List<Tuple<string, HiddenMarkovModel>> HMM;
        BaumWelchLearning teacher;

        public Worker()
        {
            FeatureVector = new List<int>();
            TrainingSequence = new List<List<int>>();
            HMM = new List<Tuple<string, HiddenMarkovModel>>();
            //teacher = new BaumWelchLearning(hmm) { Tolerance = 0.0001,Iterations = 0 };

            isTraining = true;
            onTraining = false;

            numberOfTrainingSequences = 6;

            this.kinectSensor = KinectSensor.GetDefault();

            if(kinectSensor != null)
            {
                this.kinectSensor.Open();                
            }            
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
                if(isTraining && !onTraining)
                {
                    Console.WriteLine("Do you want to add a gesture?(Y/N)");
                    string choice = Console.ReadLine();
                    if(choice == "N")
                    {
                        isTraining = false;
                        Console.WriteLine("Stand in front of the kinect and perform any gesture to recognize\n");

                    }
                    else
                    {
                        Console.Write("Enter Gesture Name: ");
                        currentGestureName = Console.ReadLine();

                        Console.WriteLine("Stand in front of the kinect and perform the gesture " + numberOfTrainingSequences + " times");
                        count = 0;
                        onTraining = true;
                    }
                }            

                foreach(Body body in this.bodies)
                {

                    if(body.IsTracked)
                    {                       

                        IReadOnlyDictionary<JointType,Joint> joints = body.Joints;
                        

                        CameraSpacePoint position = joints[JointType.HandRight].Position;
                        if(position.Z < 0)
                        {
                            position.Z = InferredZPositionClamp;
                        }


                        
                        x = (double)position.X;
                        y = (double)position.Y;
                        z = (double)position.Z;

                        if(onTraining)
                        {
                            if(tracking == false && x >= (double)0.10 && x <= (double)0.50 && y >= (double)0.15 && y <= (double)0.55)
                            {
                                tracking = true;
                                stopwatch = new Stopwatch();
                                stopwatch.Start();
                                prevx = x;
                                prevy = y;
                                prevz = z;
                                //count++;
                                //text = "Hand Right: x = " + Convert.Tostring(x) + ", y = " + Convert.Tostring(y) + "   Count: " + Convert.Tostring(count);
                                Console.WriteLine("Gesture Start!!\n\n");
                                prevTime = stopwatch.Elapsed.TotalSeconds;
                                FeatureVector.Clear();
                                System.Threading.Thread.Sleep(200);
                                continue;
                                //System.IO.File.AppendAllText(@"C:\Users\Abid\Desktop\gg\BodyBasics-WPF\WriteText.txt", text + Environment.NewLine);

                            }
                            else if(tracking)
                            {
                                currTime = stopwatch.Elapsed.TotalSeconds;
                                double speed = Math.Sqrt(((x - prevx) * (x - prevx)) + ((y - prevy) * (y - prevy)) + ((z - prevz) * (z - prevz))) * 1000.0 / (currTime - prevTime);
                                //double speed = Math.Sqrt(Math.Pow(x-prevx, 2)+Math.Pow(y-prevy, 2)+Math.Pow(z-prevz;
                                //Console.WriteLine(speed);

                                double dx, dy, theta, alpha;
                                dx = x - prevx;
                                dy = y - prevy;

                                theta = Math.Atan(dy/dx)*(180.0/Math.PI);

                                if(dy > 0 && dx > 0) alpha = theta;
                                else if(dy > 0 && dx < 0) alpha = theta+180.0;
                                else if(dy < 0 && dx < 0) alpha = theta+180.0;
                                else alpha = theta+360.0;

                                if(alpha >= 0.0 && alpha < 45.0) FeatureVector.Add(0);
                                if(alpha >= 45.0 && alpha < 90.0) FeatureVector.Add(1);
                                if(alpha >= 90.0 && alpha < 135.0) FeatureVector.Add(2);
                                if(alpha >= 135.0 && alpha < 180.0) FeatureVector.Add(3);
                                if(alpha >= 180.0 && alpha < 225.0) FeatureVector.Add(4);
                                if(alpha >= 225.0 && alpha < 270.0) FeatureVector.Add(5);
                                if(alpha >= 270.0 && alpha < 315.0) FeatureVector.Add(6);
                                if(alpha >= 315.0 && alpha < 360.0) FeatureVector.Add(7);

                                //Console.WriteLine(alpha);
                                if(speed < speedthreshold)
                                {
                                    Console.WriteLine("End of Gesture!\n\nFeature Vector: ");
                                    tracking = false;

                                    for(int i = 0;i<FeatureVector.Count;i++) Console.Write(FeatureVector[i] + ",");
                                    Console.WriteLine("");
                                    count++;
                                    Console.WriteLine(count);

                                    if(count <= numberOfTrainingSequences)
                                    {
                                        TrainingSequence.Add(FeatureVector);
                                    }
                                    if(count == numberOfTrainingSequences)
                                    {
                                        sequences = TrainingSequence.Select(a => a.ToArray()).ToArray();
                                        HiddenMarkovModel hmm = new HiddenMarkovModel(8, 8);
                                        teacher = new BaumWelchLearning(hmm) { Tolerance = 0.0001,Iterations = 0 };
                                        teacher.Run(sequences);
                                        HMM.Add(new Tuple<string, HiddenMarkovModel>(currentGestureName, hmm));

                                        Console.WriteLine("Training finished for current gesture!\n\n");
                                        onTraining = false;
                                    }                                    

                                    System.Threading.Thread.Sleep(2000);
                                }
                                prevx = x;
                                prevy = y;
                                prevz = z;
                                prevTime = currTime;
                            }
                        }

                        if(!isTraining)
                        { 
                            if(tracking == false && x >= (double)0.10 && x <= (double)0.50 && y >= (double)0.15 && y <= (double)0.55)
                            {
                                tracking = true;
                                stopwatch = new Stopwatch();
                                stopwatch.Start();
                                prevx = x;
                                prevy = y;
                                prevz = z;
                                //count++;
                                //text = "Hand Right: x = " + Convert.Tostring(x) + ", y = " + Convert.Tostring(y) + "   Count: " + Convert.Tostring(count);
                                Console.WriteLine("Gesture Start!!\n\n");
                                prevTime = stopwatch.Elapsed.TotalSeconds;
                                FeatureVector.Clear();
                                System.Threading.Thread.Sleep(200);
                                continue;
                                //System.IO.File.AppendAllText(@"C:\Users\Abid\Desktop\gg\BodyBasics-WPF\WriteText.txt", text + Environment.NewLine);

                            }
                            else if(tracking)
                            {
                                currTime = stopwatch.Elapsed.TotalSeconds;
                                double speed = Math.Sqrt(((x - prevx) * (x - prevx)) + ((y - prevy) * (y - prevy)) + ((z - prevz) * (z - prevz))) * 1000.0 / (currTime - prevTime);
                                //double speed = Math.Sqrt(Math.Pow(x-prevx, 2)+Math.Pow(y-prevy, 2)+Math.Pow(z-prevz;
                                //Console.WriteLine(speed);

                                double dx, dy, theta, alpha;
                                dx = x - prevx;
                                dy = y - prevy;

                                theta = Math.Atan(dy/dx)*(180.0/Math.PI);

                                if(dy > 0 && dx > 0) alpha = theta;
                                else if(dy > 0 && dx < 0) alpha = theta+180.0;
                                else if(dy < 0 && dx < 0) alpha = theta+180.0;
                                else alpha = theta+360.0;

                                if(alpha >= 0.0 && alpha < 45.0) FeatureVector.Add(0);
                                if(alpha >= 45.0 && alpha < 90.0) FeatureVector.Add(1);
                                if(alpha >= 90.0 && alpha < 135.0) FeatureVector.Add(2);
                                if(alpha >= 135.0 && alpha < 180.0) FeatureVector.Add(3);
                                if(alpha >= 180.0 && alpha < 225.0) FeatureVector.Add(4);
                                if(alpha >= 225.0 && alpha < 270.0) FeatureVector.Add(5);
                                if(alpha >= 270.0 && alpha < 315.0) FeatureVector.Add(6);
                                if(alpha >= 315.0 && alpha < 360.0) FeatureVector.Add(7);

                                //Console.WriteLine(alpha);
                                if(speed < speedthreshold)
                                {
                                    Console.WriteLine("End of Gesture!\n\n");
                                    tracking = false;

                                    for(int i = 0;i<FeatureVector.Count;i++) Console.Write(FeatureVector[i] + ",");
                                    Console.WriteLine("");
                                    
                                    double maximumLikelihood = 0.0;
                                    int matchedIndex = 0;
                                    for(int i=0; i<HMM.Count; i++)
                                    {
                                        double likeliHood = Math.Exp(HMM[i].Item2.Evaluate(FeatureVector.ToArray()));  
                                        
                                        if(likeliHood > maximumLikelihood)
                                        {
                                            maximumLikelihood = likeliHood;
                                            matchedIndex = i;
                                        }    
                                    }

                                    if(maximumLikelihood == 0.0) Console.WriteLine("Unidentified Gesture!!\n");
                                    else Console.WriteLine("Performed gesture identified as \""+ HMM[matchedIndex].Item1 + "\"\n\n");
                                    System.Threading.Thread.Sleep(2000);
                                }
                                prevx = x;
                                prevy = y;
                                prevz = z;
                                prevTime = currTime;
                            }
                        }
                        
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
