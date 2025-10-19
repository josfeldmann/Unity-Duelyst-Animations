using System.Collections.Generic;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class DuelystDemo : MonoBehaviour
{
    public List<RuntimeAnimatorController> controllers = new List<RuntimeAnimatorController>();

    public TextMeshProUGUI animatorText;
    public TMP_Dropdown dropDown;
    public Button buttonPrefab;
    public List<Button> spawnedButtons = new List<Button>();
    public Animator anim;

    RuntimeAnimatorController current;


    public List<Animator> anims;


    public void SetRandomAnimators() {
        foreach (Animator a in anims) {
            a.runtimeAnimatorController = controllers[Random.Range(0, controllers.Count)];
            a.Play("breathing");
        }
    }


    int index = 0;

    public void SetAnimator(int idx) {
        index = idx;
        current = controllers[idx];
        dropDown.SetValueWithoutNotify(idx);
        ShowAnimator();
    }


    [ContextMenu("Grab Controllers")]
    public void GrabFromEditor() {

#if UNITY_EDITOR
        controllers = new List<RuntimeAnimatorController>();


        string[] guids = AssetDatabase.FindAssets("t:runtimeanimatorcontroller");
        foreach (string s in guids) {
            RuntimeAnimatorController o = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(s));
            controllers.Add(o);
        }
#endif
    }
    

    public void NextAnim(int i) {
        index += i;

        if (index < 0) {
            index = controllers.Count - 1;
        } else if (index >= controllers.Count) {
            index = 0;
        }

        SetAnimator(index);


    }

    public void Randomize() {
        int idx = Random.Range(0, controllers.Count);
        SetAnimator(idx);
        
    }

    public void Awake() {
        SetRandomAnimators();

        current = controllers[0];
        index = 0;
        buttonPrefab.gameObject.SetActive(false);

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (RuntimeAnimatorController controller in controllers) {
            options.Add(new TMP_Dropdown.OptionData(controller.name));
        }

        dropDown.ClearOptions();
        dropDown.AddOptions(options);

        ShowAnimator();
    }

    public void ShowAnimator() {

        animatorText.text = current.name;

        List<AnimationClip> clips = new List<AnimationClip>(current.animationClips);

        int toSpawn = clips.Count - spawnedButtons.Count;

        for (int i = 0; i < toSpawn; i++) {
            Button b = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            spawnedButtons.Add(b);
        }

        foreach (var b in spawnedButtons) {
            b.gameObject.SetActive(false);
        }

        for (int i = 0; i < clips.Count; i++) {

            Button b = spawnedButtons[i];
            AnimationClip clip = clips[i];

            string s = clip.name.Split("_").Last();

            b.gameObject.SetActive(true);
            b.GetComponentInChildren<TextMeshProUGUI>().text = s;
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(() => PlayAnim(s));



        }

        anim.runtimeAnimatorController = current;

        anim.Play("breathing");


    }

    public void PlayAnim(string s) {
        anim.Play(s);
    }




}
