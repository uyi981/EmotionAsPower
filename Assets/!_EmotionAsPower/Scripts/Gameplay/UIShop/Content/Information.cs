using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


    public class Information : MonoBehaviour
    {
        [Header("UI References Object")]
        public Image image;
        public TextMeshProUGUI name;
        public TextMeshProUGUI description;
        public GameObject resource;
        public GameObject resourceHolder;
        private bool isLock;
        private GameObject resourceInstance;
        [SerializeField]private GameObject button;
        public IconUI selectedIconUI;
        
        public Image Image
        {
            get => image;
            set => image = value;
        }

        public TextMeshProUGUI Name
        {
            get => name;
            set => name = value;
        }
        public TextMeshProUGUI Description
        {
            get => description;
            set => description = value;
        }

        public GameObject Resource
        {
            get => resource;
            set => resource = value;
        }
        //public void OnEnable()
        //{
        //    //if(resourceInstance != null)
        //    //{
        //    //    Destroy(resourceInstance);
        //    //}
        //    if(resourceHolder.transform.childCount>0)
        //    {
        //        foreach(Transform transform in resourceHolder.transform)
        //        {
        //            Destroy(transform.gameObject);
        //        }
        //    }
        //    if(resource!=null)
        //    {
        //        resourceInstance = Instantiate(Resource, resourceHolder.transform);
        //        resourceInstance.transform.localPosition = Vector3.zero; // Reset position to avoid offset
        //        Debug.Log("Resource instantiated in Information component.");
        //    }
        //}
    private void Start()
    {
        ResetInformationData();
    }
    public void LockInformation()
    {
        isLock = !isLock;
    }
    //public void SetDataResource(GameObject ressource)
    //{
    //    IconResource[] icons = ressource.GetComponentsInChildren<IconResource>();
    //    IconResource[] iconsInstance = this.resource.GetComponentsInChildren<IconResource>();
    //    for (int i = 0; i < icons.Length; i++)
    //    {
    //        if (i < iconsInstance.Length)
    //        {
    //            iconsInstance[i].Setup(icons[i].item, icons[i].amount);
    //        }
    //        else
    //        {
    //            Debug.LogWarning("Not enough IconResource instances in the prefab to match the data.");
    //        }
    //    }
    //}
    public void SetInformation(Image img, string text, string des, GameObject resource,IconUI icon )
        {
        selectedIconUI = icon;
        if (isLock) return; // Nếu đang khóa thì không thực hiện gì cả
        button.SetActive(true);
        image.enabled = true;
        Information informationComponent = this;
            if (informationComponent == null)
            {
                Debug.LogError("Information component is missing on GameObject!");
                return;
            }

            // Kiểm tra các tham số
            if (img == null) { Debug.LogError("Image sprite is null!"); return; }
            if (text == null) { Debug.LogError("Text component is null!"); return; }
            if (des == null) { Debug.LogError("Description component is null!"); return; }
            if (informationComponent.Image == null) { Debug.LogError("Information.Image is null!"); return; }
            if (informationComponent.Name == null) { Debug.LogError("Information.Name is null!"); return; }
            if (informationComponent.Description == null) { Debug.LogError("Information.Description is null!"); return; }

            // Gán giá trị
            informationComponent.Image.sprite = img.sprite;
            informationComponent.Name.text = text;
            informationComponent.Description.text = des;
            if (resourceHolder.transform.childCount > 0)
            {
            foreach (Transform transform in resourceHolder.transform)
            {
                Destroy(transform.gameObject);
            }
            }
            informationComponent.Resource = Instantiate(resource,resourceHolder.transform);
            Resource.transform.SetParent(resourceHolder.transform);
        }
       public void ResetInformationData()
        {
            button.SetActive(false);
            image.enabled =false;
            name.text = string.Empty;
            description.text = string.Empty;
            if (resourceHolder.transform.childCount > 0)
            {
            foreach (Transform transform in resourceHolder.transform)
            {
                Destroy(transform.gameObject);
            }
            }
            resource = null;
        }
       public void Build()
        {
        if (selectedIconUI == null)
        {
            Debug.LogError("Selected IconUI is null. Cannot build.");
            return;
        }
        selectedIconUI.Place();
        }
    }
