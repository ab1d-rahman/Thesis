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

namespace DynamicGestureGUI
{
    public partial class GestureForm:Form
    {
        Dictionary <String, int> nameToLabel;
        List<List<int>> trainingSequences;
        List<int> featureVector, trainingLabels;

        public GestureForm()
        {
            InitializeComponent();
        }

        private void GestureForm_Load(object sender,EventArgs e)
        {
            trainingSequences = new List<List<int>>();
            featureVector = new List<int>();
            trainingLabels = new List<int>();

            comboTrain.SelectedItem = "Forward";

            nameToLabel = new Dictionary<string, int>();
            nameToLabel.Add("Forward", 0);
            nameToLabel.Add("Backward", 1);
            nameToLabel.Add("Speed Up", 2);
            nameToLabel.Add("Speed Down", 3);
            nameToLabel.Add("Return", 4);


        }

        private void buttonTrain_Click(object sender,EventArgs e)
        {
            labelMode.Text = "Training Mode";
            comboTrain.Visible = true;
        }

        private void buttonRecog_Click(object sender,EventArgs e)
        {
            labelMode.Text = "Recognition Mode";
            comboTrain.Visible = false;
        }

        private void buttonLearnHMM_Click(object sender,EventArgs e)
        {
            labelInfo.Text = nameToLabel[comboTrain.SelectedItem.ToString()].ToString();
        }

        private void buttonSaveToFile_Click(object sender,EventArgs e)
        {
            String sequenceFile = @"Resources\TrainingSequences.txt";
            String labelFile =  @"Resources\TrainingLabels.txt";

            System.IO.File.WriteAllText(sequenceFile, "");
            System.IO.File.WriteAllText(labelFile, "");

            for(int i=0; i<trainingSequences.Count; i++)
            {
                for(int j=0; j<trainingSequences[i].Count; j++)
                    System.IO.File.AppendAllText(sequenceFile, trainingSequences[i][j].ToString());
                System.IO.File.AppendAllText(sequenceFile, Environment.NewLine);
            }

            for(int i=0; i<trainingLabels.Count; i++)
                System.IO.File.AppendAllText(labelFile, trainingLabels[i].ToString() + Environment.NewLine);

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
}
