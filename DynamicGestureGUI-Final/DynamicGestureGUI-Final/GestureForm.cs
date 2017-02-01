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
        private Point reference, transformation;

        private const double normalizer = 15.0;

        Dictionary <int, String> labelToName;
        List<List<int>> trainingSequences;
        List<int> featureVector, trainingLabels, handX, handY, handZ;
        
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
            handX = new List<int>();
            handY = new List<int>();
            handZ = new List<int>();

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

            reference = new Point(150.0, 150.0, 150.0);

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
                            current = new Point(handRight.X*100.0-transformation.X, handRight.Y*100.0-transformation.Y, handRight.Z*100.0-transformation.Z);
                            
                            handX.Add(Convert.ToInt32(current.X/normalizer));
                            handY.Add(Convert.ToInt32(current.Y/normalizer));
                            handZ.Add(Convert.ToInt32(current.Z/normalizer));
                            
                        }

                        if(isTracking == false && body.HandLeftState == HandState.Closed)
                        {
                            labelInfo.Text = "Tracking";
                            handX.Clear();
                            handY.Clear();
                            handZ.Clear();

                            current = new Point(handRight.X*100.0, handRight.Y*100.0, handRight.Z*100.0);
                            transformation = new Point(current.X-reference.X, current.Y-reference.Y, current.Z-reference.Z);
                            current = reference;
                            handX.Add(Convert.ToInt32(current.X/normalizer));
                            handY.Add(Convert.ToInt32(current.Y/normalizer));
                            handZ.Add(Convert.ToInt32(current.Z/normalizer));
                            isTracking = true;
                            featureVector.Clear();
                            //System.Threading.Thread.Sleep(50);
                        }

                        if(isTracking == true && body.HandLeftState == HandState.Open)
                        {
                            isTracking = false;                            

                            if(isTraining == true)
                            {
                                featureVector.Clear();
                                for(int i=0; i<handX.Count; i++) featureVector.Add(handX[i]);
                                for(int i=0; i<handY.Count; i++) featureVector.Add(handY[i]);
                                for(int i=0; i<handZ.Count; i++) featureVector.Add(handZ[i]);
                                trainingSequences.Add(new List<int>(featureVector));
                                trainingLabels.Add(comboTrain.SelectedIndex);

                                //string gg ="";
                    //            for(int i = 0;i<featureVector.Count;i++)
                    //gg += featureVector[i].ToString();
                                labelInfo.Text = "Idle";
                                //Debug.WriteLine(gg);
                            }
                            else
                            {
                                featureVector.Clear();
                                for(int i=0; i<handX.Count; i++) featureVector.Add(handX[i]);
                                for(int i=0; i<handY.Count; i++) featureVector.Add(handY[i]);
                                for(int i=0; i<handZ.Count; i++) featureVector.Add(handZ[i]);
                                int recognizedLabel = classifier.Decide(featureVector.ToArray());
                                labelInfo.Text = labelToName[recognizedLabel];
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
