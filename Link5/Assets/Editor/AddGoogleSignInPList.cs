using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
//using UnityEditor.iOS.Xcode;
using UnityEngine;

//public class AddGoogleSignInPlist 
//{
//        [PostProcessBuild]
//        public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
//        {

//            if (buildTarget == BuildTarget.iOS)
//            {

//                // Get plist
//                string plistPath = pathToBuiltProject + "/Info.plist";
//                PlistDocument plist = new PlistDocument();
//                plist.ReadFromString(File.ReadAllText(plistPath));

//                // Get root
//                PlistElementDict rootDict = plist.root;

//                //// Add GoolgeSignIn Key in Xcode plist
//                var buildKey = "GoogleSignIn";
//                rootDict.SetString(buildKey, "");

//                // Write to file
//                Debug.Log("Writting pList File.");
//                File.WriteAllText(plistPath, plist.WriteToString());
//            }
//        }

//}
