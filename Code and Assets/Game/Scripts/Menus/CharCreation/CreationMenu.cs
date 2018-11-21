using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;
using schoolRPG.Stats;
using schoolRPG.Items;
using schoolRPG.SaveLoad;
using TeamUtility.IO;

namespace schoolRPG
{
    // TODO: Finish the Creation Menu when the game loads.

    // Thoughts: When the game loads, make sure to load any state from the PlayerData.
    //           When the game is Saved, make sure to save any state to the PlayerData.

    public enum ColorObj
    {
        HAIR, SHIRT, PANTS, SHOES
    }

    public enum CreateMenu
    {
        NONE, CREATE, ATTRIBUTES, SKILLS, CONFIRM
    }

    public class CreationMenu : MonoBehaviour
    {
        public static bool Creating;
        /// <summary>
        /// Set this to True when we are picking item colors with a controller
        /// </summary>
        public static bool PickingColor { get; set; }
        public bool creating;
        
        public TextMeshProUGUI display;

        public GameObject createMenu;
        public GameObject charStats;
        public GameObject attributesMenu;
        public GameObject skillsMenu;
        public GameObject confirmMenu;

        public TMP_InputField selectStart;
        public TextMeshProUGUI raceName;
        public CharStats playerStats;

        public SpriteSwitcher[] switchers;

        private ColorObj coloring;
        private bool lastPicking;
        private Selectable[] sel;

        private int menuOn;

        // Chosen Stats
        private string pName;
        private bool male;

        // Use this for initialization
        void Start()
        {
            // TESTING
            // Uncomment this when you want to create a new character when starting from the Main Game Scene
            //LoadPersistant.newGame = true;
            //pickingColor = true;
            // END TESTING

            lastPicking = false;
            raceName.text = "Light Human";
            display.text = "";
            sel = GetComponentsInChildren<Selectable>();

            // If we are loading as a new game
            if (LoadPersistant.NewGame)
            {
                menuOn = 1;
                createMenu.SetActive(true);
                charStats.SetActive(true);
                attributesMenu.SetActive(false);
                skillsMenu.SetActive(false);
                confirmMenu.SetActive(false);
                StartCoroutine(HoverSelect.SelectInput(selectStart));
                WorldController.InMenu = true;
                creating = true;
                Creating = true;
                StartCoroutine(HoverSelect.SelectSelectable(selectStart));
                //WorldController.timeScale(false);
            }
            else
            {
                createMenu.SetActive(false);
                creating = false;
                Creating = false;
            }
        }

        void Update()
        {
            if (LoadPersistant.NewGame && Creating)
            {
                // If we enter the color picker
                if (PickingColor != lastPicking && menuOn == 1) // Character Stats Menu
                {
                    lastPicking = PickingColor;
                    // If we are picking the color and not using the keyboard and mouse
                    if (PickingColor && InputManager.PlayerOneConfiguration.name != "KeyboardAndMouse")
                    {
                        for (int i = 0; i < sel.Length; ++i)
                            sel[i].interactable = false;
                    }
                    else // Otherwise, we don't need to disable the other menu items
                    {
                        for (int i = 0; i < sel.Length; ++i)
                            sel[i].interactable = true;

                        // Select the last available selectable object
                        sel[sel.Length - 1].Select();
                    }
                }

                if (InputManager.GetButton("Submit"))
                {
                    for (int i = 0; i < sel.Length; ++i)
                    {
                        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == sel[i].gameObject)
                        {
                            if(sel[i].GetComponent<TMP_InputField>() != null)
                            {
                                sel[i].GetComponent<TMP_InputField>().DeactivateInputField();
                                Selectable temp = sel[i].GetComponent<TMP_InputField>().FindSelectableOnDown();
                                StartCoroutine(HoverSelect.SelectSelectable(temp));
                            }
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Takes the player to the Main Menu
        /// </summary>
        public void MainMenu()
        {
            // Scene 0 will be the Splash screen
            SceneManager.LoadScene(1);
        }
       

        /// <summary>
        /// Select the Toggle after done editing
        /// </summary>
        /// <param name="selectTog"></param>
        public void EndInput(Toggle selectTog)
        {
            selectTog.Select();

            if (selectStart.text != "")
                pName = selectStart.text;
            else
                pName = "";

            WorldController.Data.Player.Name = pName;
        }

        /// <summary>
        /// Select Male or Female gender for the Character Creation Menu
        /// </summary>
        /// <param name="male"></param>
        public void SelectGender(Toggle tog)
        {
            // TODO: Make sure getGenderPart accounts if there is no gender equivalent item
            if (tog.isOn && tog.name.Split('T')[0] != ((playerStats.IsMale)?("Male"):("Female")))
            {
                playerStats.IsMale = !playerStats.IsMale;

                CharList.charBodyParts.ChangeGenderPart(playerStats);

                for (int i = 0; i < switchers.Length; ++i)
                {
                    switchers[i].SwitchList();
                }

                raceName.text = CharList.charBodyParts.body[playerStats.BodyPos].race;
            }

            male = playerStats.IsMale;

            if (male)
                WorldController.Data.Player.Gender = "Male";
            else
                WorldController.Data.Player.Gender = "Female";
        }

        /// <summary>
        /// Select the Next Hair style or Previous Hair style for the Character Creation Menu
        /// </summary>
        /// <param name="next"></param>
        public void SelectHair(bool next)
        {
            CharList.charBodyParts.ChangeHairPart(next, playerStats.HairPos, playerStats);

            for (int i = 0; i < switchers.Length; ++i)
                switchers[i].SwitchList();


        }

        /// <summary>
        /// Select the Next Race or Previous Race for the Character Creation Menu
        /// </summary>
        /// <param name="next"></param>
        public void SelectRace(bool next)
        {
            // Select the next race in the direction specified
            CharList.charBodyParts.ChangeRacePart(next, playerStats.BodyPos, playerStats);

            for (int i = 0; i < switchers.Length; ++i)
                switchers[i].SwitchList();

            raceName.text = CharList.charBodyParts.body[playerStats.BodyPos].race;
            WorldController.Data.Player.Race = raceName.text;
        }

        public void SelectColorChange(int co)
        {
            coloring = (ColorObj)co;
        }

        public void SetColor(Color col)
        {
            if (coloring == ColorObj.HAIR)
            {
                playerStats.hairColor = col;
                playerStats.ChangeHair(playerStats.HairPos);
                switchers[2].SwitchList();
            }
            else if (coloring == ColorObj.PANTS)
            {
                playerStats.pantsColor = col;
                playerStats.ChangePants(playerStats.PantsPos);
                switchers[4].SwitchList();
            }
            else if (coloring == ColorObj.SHIRT)
            {
                playerStats.shirtColor = col;
                playerStats.ChangeShirt(playerStats.ShirtPos);
                switchers[3].SwitchList();
            }
            else if (coloring == ColorObj.SHOES)
            {
                playerStats.shoesColor = col;
                playerStats.ChangeShoes(playerStats.ShoesPos);
                switchers[5].SwitchList();
            }
        }

        // Moves to the Next or Last creation menu
        public void NextMenu(bool next)
        {
            if (next)
                menuOn += 1;
            else if (!next && menuOn > 1)
                menuOn -= 1;

            // Call next menu and return the value of the next menu (In case we cannot go to that menu yet)
            menuOn = CallMenu(menuOn);
        }

        /// <summary>
        /// Calls the menu that is input
        /// </summary>
        /// <param name="menu"></param>
        private int CallMenu(int menu)
        {
            createMenu.SetActive(false);
            charStats.SetActive(false);
            attributesMenu.SetActive(false);
            skillsMenu.SetActive(false);
            confirmMenu.SetActive(false);


            if (menu == (int)CreateMenu.CREATE)
            {
                // Move to the Create Menu
                createMenu.SetActive(true);
                charStats.SetActive(true);
            }
            else if (menu == (int)CreateMenu.ATTRIBUTES)
            {
                createMenu.SetActive(true);
                // Move to the Attributes Menu
                if (pName != "")
                {
                    MenuController.inAttrib = false;
                    AttribMenu.setOpenMenu(true);
                    attributesMenu.SetActive(true);
                }
                else
                {
                    menu--;
                    charStats.SetActive(true);
                    StartCoroutine(DisplayMSG("<color=red>Name must have a value to continue!</color>"));
                }
            }
            else if (menu == (int)CreateMenu.SKILLS)
            {
                createMenu.SetActive(true);
                if (WorldController.Data.Player.AttributePoints == 0)
                {
                    AttribMenu.setOpenMenu(false);
                    SkillsMenu.setCreateMenu(true);
                    // Move to the Skills Menu
                    createMenu.SetActive(true);
                    skillsMenu.SetActive(true);
                }
                else
                {
                    menu--;
                    AttribMenu.setOpenMenu(true);
                    attributesMenu.SetActive(true);
                }
                
            }
            else if (menu == (int)CreateMenu.CONFIRM)
            {
                SkillsMenu.setCreateMenu(false);
                // Move to the confirmation screen
                createMenu.SetActive(true);
                confirmMenu.SetActive(true);
            }
            else
            {
                menu = (int)CreateMenu.NONE;
                WorldController.InMenu = false;
                creating = false;
                Creating = false;
                
                // Save the game for the first time
                StartCoroutine(GameObject.FindObjectOfType<WorldController>().FirstSave());
            }

            return menu;
        } // End CallMenu()

        public IEnumerator DisplayMSG(string msg)
        {
            display.text = msg;
            yield return new WaitForSeconds(5.0f);
            display.text = "";
            yield return null;
        }



    } // End Creation Menu
}

