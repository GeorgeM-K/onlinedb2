﻿// written by: Dhruvik Patel, Khalid Akash
// tested by: Dhruvik Patel, Khalid Akash
// debugged by: Dhruvik Patel, Khalid Akash
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataInsert : MonoBehaviour {
    public static string inputStudent;
    public static string inputPassword;
    public static int inputLab1Grade;
    public static int inputLab2Grade;
    public static int inputLab3Grade;

    public static int inputLab4Grade;

    public static int inputPrelab1Grade;
    public static int inputPrelab2Grade;
    public static int inputPrelab3Grade;
    public static int inputPrelab4Grade;

    public static int inputPostlab1Grade;
    public static int inputPostlab2Grade;
    public static int inputPostlab3Grade;
    public static int inputPostlab4Grade;
    public static double lab1avg;
    public static double lab2avg;
    public Boolean isDataObtained = false;
    public string[] students;
    WWW studentData;
    public string gradeOwner;
    string CreateUserURL = "http://ec2-18-204-3-220.compute-1.amazonaws.com/ReadData.php";
    StudentInfo currStudent;
    // Use this for initialization
    void Start()
    {
        StartCoroutine(GetData());
   
    }


    public void InsertStudentGrade(string student, string password, int lab1grade, int lab2grade)
    {
        if(student == "" || student == null || password == "" || password == null)
        {
            Debug.Log("Cannot insert empty student data");
            return;
        }
        WWWForm form = new WWWForm();
        form.AddField("studentPost", student);
        form.AddField("passwordPost", password);
        form.AddField("lab1gradePost", lab1grade);
        form.AddField("lab2gradePost", lab2grade);
        WWW www = new WWW(CreateUserURL, form);
        StartCoroutine(GetData());
    }

    public IEnumerator GetData()
    {
        studentData = new WWW("http://ec2-18-204-3-220.compute-1.amazonaws.com/ReadData.php");
        yield return studentData;
        string studentDataString = studentData.text;
        print(studentDataString);
        students = studentDataString.Split(';');
        lab1avg = getlab1Avg();
        lab2avg = getlab2avg();
        this.isDataObtained = true;
        Debug.Log("Data is obtained.");
        yield break;
    }

    public string GetDataValue(string data, string index)
    {
        string value = data.Substring(data.IndexOf(index) + index.Length);
        if (value.Contains("|"))
        {
            value = value.Remove(value.IndexOf("|"));
        }
        return value;
    }

    public void SetAllValues(string name)
    {
        inputStudent = name;
        inputLab1Grade = GetStudentLab1Grade(name);
        inputLab2Grade = GetStudentLab2Grade(name);
        inputPassword = GetStudentPassword(name);
    }


    public bool CheckIfStudentExists(string name)
    {
        Debug.Log("Student array length: " + students.Length);
        for(int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            Debug.Log("Student name: " + studentName);
            if (studentName == name)
            {
                Debug.Log("Student found!");
                SetAllValues(name);
                return true;
            }
        }
        return false;
    }


    public string[] getAllStudentsName()
    {
        string[] studentNames = new string[students.Length - 1];
        for (int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            Debug.Log("Student name: " + studentName);
            studentNames[i] = studentName;
        }
        return studentNames;
    }

    public List<int> getAllLab1Grades()
    {
        List<int> listOfGrades = new List<int>();
        for (int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            listOfGrades.Add(GetStudentLab1Grade(studentName));
        }
        listOfGrades.Sort();
        listOfGrades.Reverse();
        return listOfGrades;
    }

    public List<int> getAllLab2Grades()
    {
        List<int> listOfGrades = new List<int>();
        for (int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            listOfGrades.Add(GetStudentLab2Grade(studentName));
        }
        listOfGrades.Sort();
        listOfGrades.Reverse();
        return listOfGrades;
    }

    public int GetStudentLab1Grade(string name)
    {
        string grade = "";

        for (int i = 0; i < students.Length - 1; i++)
        {
            
            string value = GetDataValue(students[i], "Student:");

            if (value.Equals(name, StringComparison.Ordinal))
            {
                grade = GetDataValue(students[i], "Lab 1 Grade:");
                break;
            }
        }

        int finalGrade;
        finalGrade = Int32.Parse(grade);
        Debug.Log("Setting current user Lab 1 Grade to: " + inputLab1Grade);
        return finalGrade;
    }

    public double getlab1Avg()
    {
        double avg = 0;
        double total = 0;
        for (int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            total += GetStudentLab1Grade(studentName);
        }
        avg = total / (students.Length - 1);
        return avg;
    }

    public double getlab2avg()
    {
        double avg = 0;
        double total = 0;
        for (int i = 0; i < students.Length - 1; i++)
        {
            string studentName = GetDataValue(students[i], "Student:");
            total += GetStudentLab2Grade(studentName);
        }
        avg = total / (students.Length - 1);
        return avg;
    }

    public int GetStudentLab2Grade(string name)
    {
        string grade = "";

        for (int i = 0; i < students.Length; i++)
        {
            string value = GetDataValue(students[i], "Student:");
        if (value.Equals(name, StringComparison.Ordinal))
            {
                grade = GetDataValue(students[i], "Lab 2 Grade:");
                break;
            }
        }
        int finalGrade;
        finalGrade = Int32.Parse(grade);
        Debug.Log("Setting current user Lab 2 Grade to: " + inputLab2Grade);
        return finalGrade;
    }

    public string GetStudentPassword(string name)
    {
        string password = "";

        for (int i = 0; i < students.Length; i++)
        {
            string value = GetDataValue(students[i], "Student:");

            if (value.Equals(name, StringComparison.Ordinal))
            {
                password = GetDataValue(students[i], "Password:");
                break;
            }
        }
        return password;
    }

    public void setGradeOwner(string owner)
    {
        this.gradeOwner = owner;
    }
    

    public void PrintToCSVFormat()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string[] studentNames = this.getAllStudentsName();
        string[] lines = new string[studentNames.Length];
        for(int i = 0; i < studentNames.Length; i++)
        {
            string studentName = studentNames[i];
            string grade1 = this.GetStudentLab1Grade(studentName) + "";
            string grade2 = this.GetStudentLab2Grade(studentName) + "";
            string line = studentName  + "," + grade1  + "," + grade2 + "";
            lines[i] = line;
        }
        System.IO.File.WriteAllLines(path + "\\CSVFile.txt", lines);
    }

}

