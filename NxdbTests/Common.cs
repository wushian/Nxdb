﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Nxdb;

namespace NxdbTests
{
    [SetUpFixture]
    public class Common
    {
        public static readonly string Path = System.IO.Path.Combine(Environment.CurrentDirectory, "NxdbData");
        public const string DatabaseName = "TestDatabase";

        [SetUp]
        public void SetUp()
        {
            //Set the home path (and clear any previous test data)
            Reset();
            Database.SetHome(Path);
        }

        [TearDown]
        public void TearDown()
        {
        }

        //Remove the previous database - every test should stand in isolation
        //Need to keep trying to delete the directory because deletion fails sometimes for unknown reasons
        public static void Reset()
        {
            while (Directory.Exists(Path))
            {
                try
                {
                    Directory.Delete(Path, true);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static string GenerateXmlContent(string name)
        {
            /* <X>
             *  <XA a='Xaa'>Xa</XA>
             *  <XB d='Xbd' e='Xbe'>
             *   <XBA>Xba</XBA>
             *   <XBB a='Xbba' b='Xbbb' c='Xbbc'>Xbb</XBB>
             *  </XB>
             * </X>
             */

            return String.Format("<{0}><{0}A a='{0}aa'>{0}a</{0}A><{0}B d='{0}bd' e='{0}be'><{0}BA>{0}ba</{0}BA><{0}BB a='{0}bba' b='{0}bbb' c='{0}bbc'>{0}bb</{0}BB></{0}B></{0}>", name);
        }

        public static Documents Populate(Database database, params string[] names)
        {
            Documents documents = new Documents(new List<string>(names), new List<string>());
            using (new Update())
            {
                foreach (string name in names)
                {
                    string content = GenerateXmlContent(name);
                    documents.Content.Add(content);
                    database.Add(name, content);
                }
            }
            return documents;
        }
    }
}
