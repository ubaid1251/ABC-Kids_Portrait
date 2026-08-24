using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Ump;
using GoogleMobileAds.Ump.Api;

public class UmpManager1 : MonoBehaviour
{
    private void Awake()
    {
        
    }

    public void ConsentCall()
    {
        try
        {
            var debugSettings = new ConsentDebugSettings
            {
                // Geography appears as in EEA for debug devices.
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = new List<string>
        {
            ""
        }
            };

            // Set tag for under age of consent.
            // Here false means users are not under age.
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = true,
                ConsentDebugSettings = debugSettings,
            };
            request.ConsentDebugSettings.TestDeviceHashedIds.Add("82C4CA1DE02B800313FF5350BB87422D");
            // Check the current consent information status.
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }


    void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        if (ConsentInformation.IsConsentFormAvailable())
        {
            LoadForm();
        }
    }

    private ConsentForm _consentForm;

    void LoadForm()
    {
        // Loads a consent form.
        ConsentForm.Load(OnLoadConsentForm);
    }

    void OnLoadConsentForm(ConsentForm consentForm, FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // The consent form was loaded.
        // Save the consent form for future requests.
        _consentForm = consentForm;

        // You are now ready to show the form.
        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
        {
            _consentForm.Show(OnShowForm);
        }
    }

    void OnShowForm(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // Handle dismissal by reloading form.
        LoadForm();
    }
}