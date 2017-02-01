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

        private Point current, previous;

        Dictionary <int, String> labelToName;
        List<List<int>> trainingSequences;
        List<int> featureVector, trainingLabels;
        
        HiddenMarkovClassifier classifier;

        public GestureForm()
        {
            InitializeComponent();                   
        }

        private void GestureForm_Load(object sender,EventArgs e)
        {
            trainingSequences = new List<List<int>>();
            featureVector = new List<int>();
            trainingLabels = new List<int>();

            isTraining = false;
            isTracking = false;

            labelInfo.Text = "Idle";
            comboTrain.SelectedItem = "Forward"; 

            labelToName = new Dictionary<int, String>();
            labelToName.Add(0, "Forward");
            labelToName.Add(1, "Backward");
            labelToName.Add(2, "Speed Up");
            labelToName.Add(3, "Speed Down");
            labelToName.Add(4, "Return");

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
                        
                        if(isTracking)
                        {
                            previous = current;
                            current = new Point(handRight.X, handRight.Y, handRight.Z);
                            
                            double dx = current.X-previous.X;
                            double dy = current.Y-previous.Y;
                            
                            double theta = Math.Atan(dy/dx)*(180.0/Math.PI);
                            double alpha = 0.0;
                            
                            if(dy > 0 && dx > 0) alpha = theta;
                            else if(dy > 0 && dx < 0) alpha = theta+180.0;
                            else if(dy < 0 && dx < 0) alpha = theta+180.0;
                            else alpha = theta+360.0;

                            if(alpha >= 0.0 && alpha < 45.0) featureVector.Add(0);
                            if(alpha >= 45.0 && alpha < 90.0) featureVector.Add(1);
                            if(alpha >= 90.0 && alpha < 135.0) featureVector.Add(2);
                            if(alpha >= 135.0 && alpha < 180.0) featureVector.Add(3);
                            if(alpha >= 180.0 && alpha < 225.0) featureVector.Add(4);
                            if(alpha >= 225.0 && alpha < 270.0) featureVector.Add(5);
                            if(alpha >= 270.0 && alpha < 315.0) featureVector.Add(6);
                            if(alpha >= 315.0 && alpha < 360.0) featureVector.Add(7); 
                        }

                        if(isTracking == false && body.HandLeftState == HandState.Closed)
                        {
                            labelInfo.Text = "Tracking";
                            current = new Point(handRight.X, handRight.Y, handRight.Z);
                            isTracking = true;
                            featureVector.Clear();
                            //System.Threading.Thread.Sleep(50);
                        }

                        if(isTracking == true && body.HandLeftState == HandState.Open)
                        {
                            isTracking = false;                            

                            if(isTraining == true)
                            {
                                trainingSequences.Add(new List<int>(featureVector));
                                trainingLabels.Add(comboTrain.SelectedIndex);
                                labelInfo.Text = "Idle";
                            }
                            else
                            {
                                int recognizedLabel = classifier.Decide(featureVector.ToArray());
                                labelInfo.Text = labelToName[recognizedLabel];
                                //System.Threading.Thread.Sleep(3000);
                                String sequenceFile = @"Resources\S.txt";
                                String labelFile =  @"Resources\L.txt";

                                //System.IO.File.WriteAllText(sequenceFile, "");
                                //System.IO.File.WriteAllText(labelFile, "");

                                for(int i = 0;i<featureVector.Count;i++)
                                {

                                    System.IO.File.AppendAllText(sequenceFile,featureVector[i].ToString());
                                }

                                System.IO.File.AppendAllText(sequenceFile, "  " + recognizedLabel.ToString() + Environment.NewLine);
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
            ITopology forward = new Forward(states: 8);
            classifier = new HiddenMarkovClassifier(classes: 5, topology: forward, symbols: 8);
            var teacher = new HiddenMarkovClassifierLearning(classifier,                
                modelIndex => new BaumWelchLearning(classifier.Models[modelIndex])
                {
                    Tolerance = 0.00001, // iterate until log-likelihood changes less than 0.001
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

            //for(int i=0; i<trainingSequences.Count; i++)
            //{
            //    for(int j=0; j<trainingSequences[i].Count; j++)
            //        System.IO.File.AppendAllText(sequenceFile, trainingSequences[i][j].ToString());
            //    System.IO.File.AppendAllText(sequenceFile, Environment.NewLine);
            //}

            //for(int i=0; i<trainingLabels.Count; i++)
            //    System.IO.File.AppendAllText(labelFile, trainingLabels[i].ToString() + Environment.NewLine);
            using(StreamWriter sw = new StreamWriter(sequenceFile, true))
            {
                for(int i = 0;i<trainingSequences.Count;i++)
                {
                    for(int j = 0;j<trainingSequences[i].Count;j++)
                        sw.Write(trainingSequences[i][j].ToString());
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
                    for(int i=0; i<line.Length; i++) featureVector.Add(line[i]-48);
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

    class Point
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
