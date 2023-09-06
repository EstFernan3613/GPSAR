using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Android;

public class ARGPS : MonoBehaviour
{

    public ARSessionOrigin sessionOrigin;
    public GameObject prefab;

    public Text txtGPS;

    int maxWait = 20;

    private bool gpsEnabled = false;

    public double latitude;
    public double longitude;
    public double altitude;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateGPS());
    }

    private void Awake()
    {
        if (Application.isEditor)
        {
            // ask user permission to use the GPS
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);
            // ask user permission to use the Camera
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                Permission.RequestUserPermission(Permission.Camera);
        }
    }

    IEnumerator UpdateGPS()
    {
        // Valida si el usuario tiene el servicio de locacion activado
        if (!Input.location.isEnabledByUser)
        {
            txtGPS.text = "Servicio de Locacion: " + Input.location.isEnabledByUser;
            yield break;
        }
        else
        {
            txtGPS.text = "Servicio de Locacion" + Input.location.isEnabledByUser;
        }

        // Inicializa el servicio de localizacion
        Input.location.Start();

        // Espera hasta que el servicio de localizacion este activado
        while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
            txtGPS.text = "Esperando: " + maxWait;
        }

        if (maxWait <= 0)
        {
            txtGPS.text = "Inicializacion GPS por fuera de tiempo limite";
            yield break;
        }

        // Si el servicio de localizacion falla, detiene el script
        if(Input.location.status == LocationServiceStatus.Failed)
        {
            txtGPS.text = "Imposible de determinar la posicion del dispositivo";
            yield break;
        }
        else
        {
            gpsEnabled = true;

            txtGPS.text = "Lectura GPS Iniciada";

            // Obtiene los datos de latitud, longitud y altitud del GPS
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            altitude = Input.location.lastData.altitude;
        }

        if (gpsEnabled)
        {
            // Crea una nueva posicion en la escena basada en los datos del GPS
            Vector3 position = sessionOrigin.transform.position;
            position.x = (float)longitude;
            position.y = 0;
            position.z = (float)latitude;

            // Posiciona el objeto marcador en la posicion del GPS
            prefab.transform.position = position;

            // Calcula la orientacion del objeto marcador basado en la direccion del dispositivo
            Vector3 forward = sessionOrigin.transform.forward;
            forward.y = 0;
            Quaternion rotation = Quaternion.LookRotation(forward);
            prefab.transform.rotation = rotation;

            // Mostramos los datos del GPS en la interfaz
            txtGPS.text = "Long: " + position.x.ToString() + "\n" + "Alt: " +
                position.y.ToString() + "\n" + "Lat: " + position.z.ToString()
                + "\n \n" + prefab.transform.position;

            Input.location.Stop();

        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
