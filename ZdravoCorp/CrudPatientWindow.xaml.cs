﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ZdravoCorp
{
    /// <summary>
    /// Interaction logic for NurseWindow.xaml
    /// </summary>
    public partial class CrudPatientWindow : Window
    {

        List<MedicalRecord> records;
        List<Patient> patients;
        private Nurse nurse;
        public CrudPatientWindow(Nurse nurse)
        {
            InitializeComponent();
            this.nurse = nurse;
            LoadData();
        }

        public DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Patient ID");
            dt.Columns.Add("First name");
            dt.Columns.Add("Last name");
            dt.Columns.Add("Birth date");
            dt.Columns.Add("Username");
            dt.Columns.Add("Password");
            dt.Columns.Add("Medical Record ID");
            dt.Columns.Add("Height");
            dt.Columns.Add("Weight");
            dt.Columns.Add("Anamnesis");
            dt.AcceptChanges();
            return dt;
        }

        public void LoadData()
        {
            DataTable dt = CreateDataTable();
            records = Singleton.Instance.medicalRecords;
            patients = Singleton.Instance.patients;
            foreach (Patient patient in patients)
            {
                foreach (MedicalRecord record in records)
                {
                    if (patient.MedicalRecordId == record.Id)
                    {
                        dt.Rows.Add(patient.Id, patient.FirstName, patient.LastName, patient.BirthDate,
                            patient.Username, patient.Password,record.Id, record.Height, record.Weight, record.EarlierIllnesses);
                        dt.AcceptChanges();
                    }
                }
            }
            datagrid.DataContext = dt.DefaultView;

        }
        public void createButton(object sender, RoutedEventArgs e)
        {
            //CreateMedicalRecordWindow createMedicalRecordWindow = new CreateMedicalRecordWindow(true,null,false);
            //createMedicalRecordWindow.ShowDialog();
            CreatePatientWindow createPatientWindow = new CreatePatientWindow(true, null, false);
            createPatientWindow.ShowDialog();
            LoadData();
        }

        public void updateButton(object sender, RoutedEventArgs e)
        {
            int selectedIndex = datagrid.SelectedIndex;

            if (selectedIndex == -1)
            {
                MessageBox.Show("You must select the patient whose account you want to edit.", "Warning", (MessageBoxButtons)MessageBoxButton.OK, (MessageBoxIcon)MessageBoxImage.Warning);
                
            }
            else
            {
                Patient selectedPatient = Singleton.Instance.patients[selectedIndex];
                //CreateMedicalRecordWindow createMedicalRecordWindow = new CreateMedicalRecordWindow(false,selectedPatient , false);
                //createMedicalRecordWindow.ShowDialog();
                CreatePatientWindow createPatientWindow = new CreatePatientWindow(false, selectedPatient, false);
                createPatientWindow.ShowDialog();
                LoadData();

            }
        }
        public void deleteButton(object sender, RoutedEventArgs e)
         {
            int selectedIndex = datagrid.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("You must select the patient whose account you want to edit.", "Warning", (MessageBoxButtons)MessageBoxButton.OK, (MessageBoxIcon)MessageBoxImage.Warning);

            }
            else
            {

                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this patient? ", "Question", MessageBoxButtons.YesNo);
                if (dialogResult.ToString() == "Yes")
                {
                    Patient selectedPatient = Singleton.Instance.patients[selectedIndex];
                    MedicalRecord selectedRecord = null;
                    int recordId = selectedPatient.MedicalRecordId;
                    foreach (MedicalRecord record in Singleton.Instance.medicalRecords)
                    {
                        if (recordId == record.Id)
                        {
                            selectedRecord = record;
                            break;
                        }
                    }

                    Singleton.Instance.patients.Remove(selectedPatient);
                    Singleton.Instance.medicalRecords.Remove(selectedRecord);
                    User.RemoveUser(selectedPatient.Username);
                    User.WriteAll(Singleton.Instance.users);
                    selectedPatient.WriteAll(Singleton.Instance.patients);
                    selectedRecord.WriteAll(Singleton.Instance.medicalRecords);
                    LoadData();
                }
            }
            
        }

    }
}
