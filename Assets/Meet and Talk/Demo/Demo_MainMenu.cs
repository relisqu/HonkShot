using UnityEngine;
using UnityEngine.SceneManagement;
using MeetAndTalk.GlobalValue;

namespace MeetAndTalk.Demo
{
    public class Demo_MainMenu : MonoBehaviour
    {
        public TMPro.TMP_InputField NameInput;
        public GlobalValueClass NameGlobalValue;

        GlobalValueManager manager;

        public void Awake()
        {
            manager = Resources.Load<GlobalValueManager>("GlobalValue");
            manager.LoadFile();

            NameInput.text = manager.Get(NameGlobalValue.ValueName);
        }

        public void DemoChangeValue(string Test)
        {
            manager.Set(NameGlobalValue.ValueName, NameInput.text);
        }

        public void OpenDemo(int ID)
        {
            SceneManager.LoadScene(ID);
        }

        //public 
    }
}
