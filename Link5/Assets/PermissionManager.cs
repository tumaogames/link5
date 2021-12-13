using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionManager : MonoBehaviour
{
    private static bool ShouldAskContactsPermission;
    private static bool ShouldAskPhonePermission;
    private static bool ContactsPermissionGranted;
    private static bool PhonePermissionGranted;

    private void Awake()
    {
        AndroidRuntimePermissions.Permission readContacts =  AndroidRuntimePermissions.CheckPermission("android.permission.READ_CONTACTS");
        if(readContacts == AndroidRuntimePermissions.Permission.ShouldAsk || readContacts == AndroidRuntimePermissions.Permission.Denied)
        {
            ShouldAskContactsPermission = true;
        } 
        else
        {
            ContactsPermissionGranted = true;
        }
        AndroidRuntimePermissions.Permission phone = AndroidRuntimePermissions.CheckPermission("android.permission.READ_PHONE_STATE");
        if (readContacts == AndroidRuntimePermissions.Permission.ShouldAsk || readContacts == AndroidRuntimePermissions.Permission.Denied)
        {
            ShouldAskPhonePermission = true;
        } 
        else
        {
            PhonePermissionGranted = true;
        }
    }

    public static void RequestPermissons()
    {
        if(ShouldAskContactsPermission)
        {
            AndroidRuntimePermissions.Permission readContactsPermissionResult = AndroidRuntimePermissions.RequestPermission("android.permission.READ_CONTACTS");
            if(readContactsPermissionResult == AndroidRuntimePermissions.Permission.Granted)
            {
                ContactsPermissionGranted = true;
            }
        }
        if (ShouldAskPhonePermission)
        {
            AndroidRuntimePermissions.Permission phonePermissionResult = AndroidRuntimePermissions.RequestPermission("android.permission.READ_PHONE_STATE");
            if (phonePermissionResult == AndroidRuntimePermissions.Permission.Granted)
            {
                PhonePermissionGranted = true;
            }
        }
    }

    public static bool HasPermissions()
    {
        return PhonePermissionGranted && ContactsPermissionGranted;
    }

    public static void ShowSettings()
    {
        AndroidRuntimePermissions.OpenSettings();
    }
}
