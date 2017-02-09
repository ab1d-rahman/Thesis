using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Kinect;

using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;


namespace DynamicGestureGUI
{
    public partial class GestureForm:Form
    {
        private const float InferredZPositionClamp = 0.1f;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        
        private bool isTraining;
        private bool isTracking;
        private bool isKinectOn;
        private bool goForward;

        private Point current, previous;
        private Point reference, transformation;
        private Point start, end, robot, intersection;
        
        private StreamWriter simulator;
        private const double normalizer = 20.0;
        private const double speedChange = 2.0;
        private double A, B, D;
        private double speed;

        Dictionary <int, String> labelToName;
        List<List<int>> trainingSequences;
        List<int> featureVector, trainingLabels, handX, handY, handZ;
        
        HiddenMarkovClassifier classifier;

        int px, py, pz;

        public GestureForm()
        {
            InitializeComponent();                   
        }

        private void GestureForm_Load(object sender,EventArgs e)
        {
            trainingSequences = new List<List<int>>();
            featureVector = new List<int>();
            trainingLabels = new List<int>();
            handX = new List<int>();
            handY = new List<int>();
            handZ = new List<int>();

            isTraining = false;
            isTracking = false;
            isKinectOn = false;
            goForward = true;

            A = B = D = 500.0;

            speed = 2.0;
            labelGesture.Text = "Idle";
            comboTrain.SelectedItem = "Forward"; 
            comboPointing.SelectedItem = "Method 1";

            labelToName = new Dictionary<int, String>();
            labelToName.Add(0, "Forward");
            labelToName.Add(1, "Backward");
            labelToName.Add(2, "Speed Up");
            labelToName.Add(3, "Speed Down");
            labelToName.Add(4, "Return");

            System.IO.File.WriteAllText(@"Simulator/data.txt","");
            simulator = new StreamWriter(@"Simulator/data.txt", true);

            reference = new Point(200.0, 200.0, 200.0);
            robot = new Point(0.0, 0.0);
            intersection = new Point(0.0, 0.0);

            this.kinectSensor = KinectSensor.GetDefault();

            if(kinectSensor != null)
            {
                this.kinectSensor.Open();
                //this.kinectSensor.Close();
            }

            GetFrames();    
            
        }

        public void GetFrames()
        {
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;            
        }

        public double distance(Point a, Point b)
        {
            return Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Z-b.Z)*(a.Z-b.Z));
        }
        
        public void forward()
        {
            goForward = true;
        }

        public void backward()
        {
            goForward = false;
        }

        public void speedUp()
        {
            speed += speedChange;
        }

        public void speedDown()
        {
            if(speed >=0.0) speed -= speedChange;
        }
        private void buttonKinect_Click(object sender,EventArgs e)
        {
            if(isKinectOn == false)
            {
                isKinectOn = true;
                buttonKinect.Text = "Stop Kinect";
                if(kinectSensor != null)
                {
                    this.kinectSensor.Open();   
                    //this.kinectSensor.Close();
                }
            }
            else
            {
                isKinectOn = false;
                buttonKinect.Text = "Start Kinect";
                if(kinectSensor != null)
                {
                    //this.kinectSensor.Open();   
                    this.kinectSensor.Close();
                }
            }
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
                        CameraSpacePoint handRight = joints[JointType.HandRight].Position;
                        CameraSpacePoint shoulderRight = joints[JointType.ShoulderRight].Position;
                        CameraSpacePoint elbowRight = joints[JointType.ElbowRight].Position;
                        CameraSpacePoint handLeft = joints[JointType.HandLeft].Position;
                        CameraSpacePoint elbowLeft = joints[JointType.ElbowLeft].Position;
                        

                        /// Pointing Gesture
                        //Debug.WriteLine(handRight.Y*100.0 + " " + elbowRight.Y*100 + " " + shoulderRight.Y*100);
                        double handRightY = handRight.Y * 100.0;
                        double elbowRightY = elbowRight.Y * 100.0;
                        double shoulderRightY = shoulderRight.Y * 100.0;

                        if(handRightY > shoulderRightY && elbowRightY > shoulderRightY && Math.Abs(elbowRightY-shoulderRightY) > 5.0  && Math.Abs(elbowRightY-shoulderRightY) < 13.0  && Math.Abs(handRightY-shoulderRightY) >15.0 && Math.Abs(handRightY-shoulderRightY) < 26.0)
                        {
                            labelRobot.Text = "Robot Status: Moving";

                            if(comboPointing.SelectedIndex == 0)
                            {
                                start = new Point((double)shoulderRight.X*100.0,(double)shoulderRight.Z*100.0);
                                end = new Point((double)handRight.X*100.0,(double)handRight.Z*100.0);


                                //Console.WriteLine(start.X + " " + start.Z + " " + end.X + " " + end.Z);

                                // calculating slope(m) and intercept(c)
                                double dz = end.Z - start.Z;
                                double dx = end.X - start.X;

                                double m = dz/dx;
                                double c = start.Z-m*start.X;
                                double x, z;
                                if(dx == 0.0)
                                {
                                    x = end.X;
                                    z = -D;
                                }
                                else if(dx > 0.0)
                                {
                                    z = m*A+c;
                                    if(z < -D)
                                    {
                                        z = -D;
                                        x = (z-c)/m;
                                    }
                                    else x = A;
                                }
                                else
                                {
                                    z = (m*(-B))+c;
                                    if(z < -D)
                                    {
                                        z = -D;
                                        x = (z-c)/m;
                                    }
                                    else x = -B;
                                }
                                
                                simulator.WriteLine("s " + start.X + " " + start.Z);
                                simulator.WriteLine("p " + x + " " + z);
                                simulator.WriteLine("i " + x + " " + z);
                                double length = speed;

                                double alpha = Math.Atan2(z-robot.Z, x-robot.X);
                                robot = new Point(robot.X+length*Math.Cos(alpha),robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position
                                simulator.WriteLine("r " + robot.X + " " + robot.Z);

                            }
                            else
                            {
                                start = new Point((double)shoulderRight.X*100.0,(double)shoulderRight.Z*100.0);
                                end = new Point((double)handRight.X*100.0,(double)handRight.Z*100.0);
                                                                
                                // calculating slope(m) and intercept(c)
                                double dz = end.Z - start.Z;
                                double dx = end.X - start.X;

                                double m1 = dz/dx;
                                double c1 = start.Z-m1*start.X;

                                double m2 = -(1/m1);
                                double c2 = robot.Z-(robot.X*m2);

                                intersection.X = (c2-c1)/(m1-m2);
                                intersection.Z = m1*intersection.X+c1;

                                double length = 800.0;
                                double alpha = Math.Atan2(end.Z-start.Z,end.X-start.X);
                                end = new Point(start.X+length*Math.Cos(alpha),start.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position


                                simulator.WriteLine("s " + start.X + " " + start.Z);
                                simulator.WriteLine("p " + end.X + " " + end.Z);
                                simulator.WriteLine("i " + intersection.X + " " + intersection.Z);
                                
                                
                                if(distance(intersection, robot) > 10.0)
                                {
                                    length = 5.0;
                                    alpha = Math.Atan2(intersection.Z-robot.Z, intersection.X-robot.X);
                                    robot = new Point(robot.X+length*Math.Cos(alpha), robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position
                                                   
                                }
                                else
                                {
                                    if(goForward)
                                    {
                                        length = speed;
                                        alpha = Math.Atan2(end.Z-robot.Z, end.X-robot.X);
                                        robot = new Point(robot.X+length*Math.Cos(alpha), robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position                               
                                    }
                                    else
                                    {
                                        length = speed;
                                        alpha = Math.Atan2(start.Z-robot.Z, start.X-robot.X);
                                        robot = new Point(robot.X+length*Math.Cos(alpha), robot.Z+length*Math.Sin(alpha));  // extending the line by length cm from start position                               
                                    
                                    }
                                    
                                }

                                simulator.WriteLine("r " + robot.X + " " + robot.Z);                                
                            }
                        }
                        else
                        {
                            labelRobot.Text = "Robot Status: Idle";
                        }

                        if(isTracking)
                        {
                            current = new Point(handRight.X*100.0-transformation.X,handRight.Y*100.0-transformation.Y,handRight.Z*100.0-transformation.Z);
                            //Debug.WriteLine(current.X + " " + current.Y + " " + current.Z);
                            //Debug.WriteLine(Convert.ToInt32(current.X/normalizer));
                            handX.Add(Convert.ToInt32(current.X/normalizer));
                            handY.Add(Convert.ToInt32(current.Y/normalizer));
                            handZ.Add(Convert.ToInt32(current.Z/normalizer));

                        }

                        if(isTracking == false && elbowLeft.Y*100.0+14.0 < handLeft.Y*100.0 &&  body.HandLeftState == HandState.Closed)
                        {
                            labelGesture.Text = "Tracking";
                            handX.Clear();
                            handY.Clear();
                            handZ.Clear();

                            current = new Point(handRight.X*100.0,handRight.Y*100.0,handRight.Z*100.0);
                            transformation = new Point(current.X-reference.X,current.Y-reference.Y,current.Z-reference.Z);
                            current = reference;
                            handX.Add(Convert.ToInt32(current.X/normalizer));
                            handY.Add(Convert.ToInt32(current.Y/normalizer));
                            handZ.Add(Convert.ToInt32(current.Z/normalizer));
                            isTracking = true;
                            featureVector.Clear();
                        }

                        if(isTracking == true && body.HandLeftState == HandState.Open)
                        {
                            isTracking = false;

                            if(isTraining == true)
                            {
                                featureVector.Clear();
                                for(int i = 0;i<handX.Count;i++) featureVector.Add(handX[i]);
                                featureVector.Add(0);
                                for(int i = 0;i<handY.Count;i++) featureVector.Add(handY[i]);
                                featureVector.Add(0);
                                for(int i = 0;i<handZ.Count;i++) featureVector.Add(handZ[i]);
                                featureVector.Add(0);
                                trainingSequences.Add(new List<int>(featureVector));
                                trainingLabels.Add(comboTrain.SelectedIndex);
                                
                                labelGesture.Text = "Idle";
                            }
                            else
                            {
                                featureVector.Clear();
                                for(int i = 0;i<handX.Count;i++) featureVector.Add(handX[i]);
                                for(int i = 0;i<handY.Count;i++) featureVector.Add(handY[i]);
                                for(int i = 0;i<handZ.Count;i++) featureVector.Add(handZ[i]);
                                int recognizedLabel = classifier.Decide(featureVector.ToArray());
                                labelGesture.Text = labelToName[recognizedLabel];
                                
                                if(recognizedLabel == 0) forward();
                                else if(recognizedLabel == 1) backward();
                                else if(recognizedLabel == 2) speedUp();
                                else if(recognizedLabel == 3) speedDown();
                                
                                //String sequenceFile = @"Resources\S.txt";
                                //String labelFile =  @"Resources\L.txt";

                                ////System.IO.File.WriteAllText(sequenceFile, "");
                                ////System.IO.File.WriteAllText(labelFile, "");

                                //for(int i = 0;i<featureVector.Count;i++)
                                //{

                                //    System.IO.File.AppendAllText(sequenceFile,featureVector[i].ToString());
                                //}

                                //System.IO.File.AppendAllText(sequenceFile, "  " + recognizedLabel.ToString() + Environment.NewLine);
                            }
                        }
                    }
                }                
            }
        }

        private void buttonTrain_Click(object sender,EventArgs e)
        {
            labelMode.Text = "Training Mode";
            comboTrain.Visible = true;
            isTraining = true;
        }

        private void buttonRecog_Click(object sender,EventArgs e)
        {
            labelMode.Text = "Recognition Mode";
            comboTrain.Visible = false;
            isTraining = false;
        }

        private void buttonLearnHMM_Click(object sender,EventArgs e)
        {
            ITopology forward = new Forward(states: 6);
            classifier = new HiddenMarkovClassifier(classes: 5, topology: forward, symbols: 20);
            var teacher = new HiddenMarkovClassifierLearning(classifier,                
                modelIndex => new BaumWelchLearning(classifier.Models[modelIndex])
                {
                    Tolerance = 0.0001, // iterate until log-likelihood changes less than 0.001
                    Iterations = 0     // don't place an upper limit on the number of iterations
                });

            int[][] inputSequences = trainingSequences.Select(a => a.ToArray()).ToArray();
            int[] outputLabels = trainingLabels.ToArray();

            double error = teacher.Run(inputSequences, outputLabels);
        }

        private void buttonSaveToFile_Click(object sender,EventArgs e)
        {
            String sequenceFile = @"Resources\TrainingSequences.txt";
            String labelFile =  @"Resources\TrainingLabels.txt";

            System.IO.File.WriteAllText(sequenceFile,"");
            System.IO.File.WriteAllText(labelFile,"");

            using(StreamWriter sw = new StreamWriter(sequenceFile, true))
            {
                for(int i = 0;i<trainingSequences.Count;i++)
                {
                    for(int j = 0;j<trainingSequences[i].Count;j++)
                        sw.Write(trainingSequences[i][j].ToString() + " ");
                    sw.WriteLine("");
                }                
            }

            using(StreamWriter sw = new StreamWriter(labelFile, true))
            {
                for(int i = 0;i<trainingLabels.Count;i++)
                    sw.WriteLine(trainingLabels[i].ToString());
            }


        }

        private void buttonLoadFromFile_Click(object sender,EventArgs e)
        {
            String sequenceFile = @"Resources\TrainingSequences.txt";
            String labelFile =  @"Resources\TrainingLabels.txt";

            using(StreamReader sr = File.OpenText(sequenceFile))
            {
                trainingSequences.Clear();

                String line;
                while((line = sr.ReadLine()) != null)
                {
                    featureVector.Clear();
                    String[] fv = line.Split(' ');
                    for(int i=0; i<fv.Length-1; i++) featureVector.Add(Convert.ToInt32(fv[i]));
                    trainingSequences.Add(new List<int>(featureVector));
                }
            }

            using(StreamReader sr = File.OpenText(labelFile))
            {      
                trainingLabels.Clear(); 
                         
                String line;
                while((line = sr.ReadLine()) != null)
                {                    
                    trainingLabels.Add(line[0]-48);
                }
            }
        }
    }

    public class Point
    {
        public double X, Y, Z;
        public Point(double xx, double yy, double zz)
        {
            this.X = xx;
            this.Y = yy;
            this.Z = zz;
        }
        public Point(double xx,double zz)
        {
            this.X = xx;
            this.Z = zz;
        }
    }
}
