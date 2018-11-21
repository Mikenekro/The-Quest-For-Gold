using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Quests;
using TMPro;
using UnityEngine.UI;
using TeamUtility.IO;
using schoolRPG.Items;
using schoolRPG.Stats;

namespace schoolRPG.Dialogue
{
    /// <summary>
    /// Controls what is displayed on the Dialogue Menu
    /// </summary>
    public class DialogueMenuController : MonoBehaviour
    {
        /// <summary>
        /// Is the DMC Open?
        /// </summary>
        public static bool IsOpen;
        public static NpcController npcC;
        /// <summary>
        /// Can we close this menu or is the closing temporary?
        /// </summary>
        public static bool CanClose;

        public GameObject choices;
        public GameObject npcDialogue;
        public GameObject tradeMenu;
        public TextMeshProUGUI npcName;
        public TextMeshProUGUI npcResponse;

        public GameObject contentArea;
        public Button baseChoiceBtn;

        private int btnSelect;
        /// <summary>
        /// Skips the current NPC response Text
        /// </summary>
        private bool skipText;
        private bool canSkip;
        private bool ending;
        private bool openMerch;
        private bool passCheck = true;
        private bool passCheckQ = true;
        private bool autoSave;
        private NPCDialogue npc;
        private List<Button> btns;
        

        private void Awake()
        {
            CanClose = true;
            btnSelect = 0;
            btns = new List<Button>();
            npcDialogue.SetActive(false);
            choices.SetActive(false);
            tradeMenu.SetActive(false);
            baseChoiceBtn.gameObject.SetActive(false);
        }

        // Use this for initialization
        private void OnEnable()
        {
            skipText = false;
            btnSelect = 0;
            ending = false;
            contentArea.transform.localPosition = new Vector3(contentArea.transform.localPosition.x, 0, contentArea.transform.localPosition.z);

            if (WorldController.Data.CanAutosave)
            {
                autoSave = true;
                WorldController.Data.CanAutosave = false;
            }

            if (npcC != null)
            {
                // If we find an NPC Dialogue on the NPC that is requesting this Dialogue Menu
                if (npcC.gameObject.GetComponent<NPCDialogue>() != null)
                {
                    npc = npcC.gameObject.GetComponent<NPCDialogue>();
                    npcName.text = npcC.NPC.Name;
                    MenuController.bottomUI.SetActive(false);

                    // Loads the Dialogue Options for this NPC's Greeting and Greets the player
                    StartCoroutine(ProcessSpeak(null, npc.Greet));
                }
            }

        }
        private void OnDisable()
        {
            int i;

            if (CanClose)
            {
                // Reset the game to its original state
                IsOpen = false;
                npcDialogue.SetActive(false);
                if (npcC != null && npcC.NPC != null)
                    npcC.NPC.IsTalking = false;
                choices.SetActive(false);
                npcDialogue.SetActive(false);
                WorldController.InMenu = false;

                if (MenuController.bottomUI == null)
                    MenuController.bottomUI = GameObject.Find("BottomUI");

                MenuController.bottomUI.SetActive(true);

                if (autoSave)
                    WorldController.Data.CanAutosave = true;

                // Set values to null
                npcC = null;
                npc = null;
                if (btns != null)
                {
                    for (i = 0; i < btns.Count; ++i)
                    {
                        if (Application.isEditor)
                            DestroyImmediate(btns[i].gameObject);
                        else
                            Destroy(btns[i].gameObject);
                    }
                    btns.Clear();
                }
            }
        }

        private void Update()
        {
            // Allow the current text to Skip
            if (canSkip && !skipText && (InputManager.GetButtonDown("Submit") || InputManager.GetButtonDown("Attack")))
            {
                skipText = true;
            }

            // Allow the Dialogue to close
            if (InputManager.GetButtonDown("Pause") && CanClose)
            {
                ending = true;

                // Make sure we are not active
                if (gameObject.activeInHierarchy)
                    gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Gets called when an option is selected
        /// <paramref name="nextID">The Dialogue Option that the Player selected</paramref>
        /// </summary>
        public void GetResponse(DialogueOption option, int btnNum)
        {
            int next;
            int i;
            btnSelect = btnNum;

            // Increase the number of conversations the player has had
            WorldController.Data.StatValue.TotalConversations += 1;

            // Don't allow button interactions until the NPC finishes speaking
            for (i = 0; i < btns.Count; ++i)
                btns[i].interactable = false;

            
            // Get the next Dialogue Location in the Dialogue List
            next = npc.GetNextDialogue(option, !passCheck);
            // Reset pass check after it's used
            if (!passCheck)
                passCheck = true;

            // Get the Dialogue and the Available Dialogue Options
            Dialogue d = npc.DialogueList[next];

            // Display the Response and load the new Dialogue
            StartCoroutine(ProcessSpeak(d));
        }

        /// <summary>
        /// Processes what the NPC says and displays it to the screen. 
        /// If there is more than 1 sentence, it will be broken down into multiple text lines that are displayed independently
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessSpeak(Dialogue d = null, Greeting g = null)
        {
            WaitForSeconds wfs = new WaitForSeconds(0.1f);
            float curTime = 0;
            float totalTime = 2.0f;
            int i;
            int lines;
            int curLine = 0;
            bool changedWait = false;
            string text = "";
            string[] display;

            if (d != null)
                text = d.Response;
            else if (g != null)
                text = g.Greet;

            display = text.Split('.');
            lines = display.Length;

            // Display each Line of text for the Dialogue
            while (curLine < lines)
            {
                // Make sure the Dialogue is active
                if (!npcDialogue.activeInHierarchy)
                    npcDialogue.SetActive(true);

                // Allow tripple periods after the ending period
                if (display[curLine].Contains(";p;"))
                    display[curLine] = display[curLine].Replace(";p;", "...");

                npcResponse.text = display[curLine];
                curLine++;
                // Don't allow any of the conditions below to be displayed on their own line
                if (curLine < lines)
                {
                    if (display[curLine].Trim() == "" || display[curLine] == null)
                        curLine++;
                }

                // Display longer text chunks for a longer amount of time
                if (npcResponse.text.Length > 20)
                {
                    totalTime = (float)(npcResponse.text.Length / 10.0f);
                    changedWait = true;
                }
                else if (changedWait)
                {
                    totalTime = 2.0f;
                    changedWait = false;
                }

                // Reset time spent in Dialogue
                curTime = 0.0f;
                canSkip = true;

                // Wait in 0.1f iterations so we can detect when the Player presses a button to skip the text
                while (curTime < totalTime)
                {
                    // If the player wants to skip the text
                    if (skipText)
                    {
                        skipText = false;
                        break;
                    }
                    // Increase the time spent in Dialogue
                    curTime += 0.1f;
                    yield return wfs;
                }

                canSkip = false;
                
            }

            // Make sure the choices are active
            if (!choices.activeInHierarchy)
                choices.SetActive(true);

            if (openMerch)
            {
                openMerch = false;
                tradeMenu.SetActive(true);
                gameObject.SetActive(false);
                yield return null;
            }
            else
            {
                // Load the Options after the character has spoken
                if (d != null)
                {
                    CanClose = false;
                    LoadOptions(d);
                }
                else if (g != null)
                {
                    CanClose = true;
                    LoadOptions(g);
                }

                // Allow button interactions
                for (i = 0; i < btns.Count; ++i)
                    btns[i].interactable = true;

                if (ending)
                {
                    // Make sure we are not active
                    if (gameObject.activeInHierarchy)
                        gameObject.SetActive(false);
                }
            }
            
            yield return null;
        }

        
        /// <summary>
        /// Load Available Options for the Greeting
        /// </summary>
        /// <param name="g"></param>
        private void LoadOptions(Greeting g)
        {
            // Retrieve all the Available Dialoge Options for the input Greeting
            LoadOptions(npc.GetDialogueOptions(g));
        }

        /// <summary>
        /// Load Available Options for Dialogue
        /// </summary>
        /// <param name="d"></param>
        private void LoadOptions(Dialogue d)
        {
            int i;

            // Simulate the Player gathering new information that allows them to ask the NPC about
            if (d.AddToGreet.Count > 0)
            {
                // Add each Dialogue Option to the Greeting menu
                for (i = 0; i < d.AddToGreet.Count; ++i)
                    npc.Greet.Options.Add(d.AddToGreet[i]);

                // Clear the options so they are not added again
                d.AddToGreet.Clear();
            }

            CanClose = false;
            // If the Dialogue does not have more options, return to the Greeting Options screen
            if (!LoadOptions(npc.GetDialogueOptions(d)))
            {
                CanClose = true;
                LoadOptions(npc.GetDialogueOptions(npc.Greet));
            }
        }

        /// <summary>
        /// Loads the Dialogue Options
        /// </summary>
        private bool LoadOptions(List<DialogueOption> options)
        {
            int i;
            bool good = true;

            if (btns == null)
                btns = new List<Button>();

            for (i = 0; i < btns.Count; ++i)
            {
                if (Application.isEditor)
                    DestroyImmediate(btns[i].gameObject);
                else
                    Destroy(btns[i].gameObject);
            }
            btns.Clear();

            // Loop through each Available Options
            for (i = 0; i < options.Count; ++i)
            {
                int btnNum = i;
                Button btn;
                DialogueOption option = options[i];
                GameObject obj;

                obj = Instantiate(baseChoiceBtn.gameObject, contentArea.transform, true);
                obj.SetActive(true);

                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y - (50.0f * i), obj.transform.transform.localPosition.z);

                btn = obj.GetComponent<Button>();

                // If this Dialogue Option is a one time only option that hasn't been used, add a one-time-only listener
                if (option.IsOneTime && !option.UsedOneTime)
                    btn.onClick.AddListener(() => { UsedOneTime(option); });
                // If this Option will start the Trading Menu for this NPC
                if (option.StartsMerchant)
                    btn.onClick.AddListener(() => { StartMerchant(option); });
                // If this Option requires Skill to be successful
                if (option.SkillRequire != SkillCheck.NONE)
                    btn.onClick.AddListener(() => { CheckSkillRequirement(option); });
                // If this Option is related to a Quest
                if (option.IsQuestOption)
                    btn.onClick.AddListener(() => { SetQuestStage(option); });
                if (option.ChangeOpinion)
                    btn.onClick.AddListener(() => { ChangeOpinion(option); });
                // If this Option will end the Dialogue
                if (option.EndsDialogue)
                    btn.onClick.AddListener(() => { StartCoroutine(EndsDialogue(option)); });

                obj.GetComponentInChildren<TextMeshProUGUI>().text = options[i].OptionText;

                // Set the function that will run if we click on the item
                obj.GetComponent<Button>().onClick.AddListener(() => { GetResponse(option, btnNum); });
                btns.Add(obj.GetComponent<Button>());

                if (i >= 10)
                {
                    contentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea.GetComponent<RectTransform>().sizeDelta.x,
                        contentArea.GetComponent<RectTransform>().sizeDelta.y + 50.0f);
                }
            }

            if (btns.Count > 0)
            {
                // Make sure we are selecting a valid button
                if (btnSelect >= btns.Count)
                    btnSelect = 0;

                // Select the first option in the list
                StartCoroutine(HoverSelect.SelectFirstButton(btns[btnSelect]));
            }
            else
                good = false;

            return good;
        }
        
        /// <summary>
        /// Changes this NPCs opinion of the Player and other NPCs
        /// </summary>
        /// <param name="option"></param>
        public void ChangeOpinion(DialogueOption option)
        {
            if (option.ChangeOpinion)
            {
                npcC.SetRelation(option.NewOpinion);
                if (option.NewOpinion != NPCType.ENEMY)
                    npcC.ForceAttack(false);
                else if (option.NewOpinion == NPCType.ENEMY)
                    npcC.ForceAttack(true);
            }
        }

        /// <summary>
        /// Sets the next Quest Stage after this option is selected
        /// </summary>
        /// <param name="option"></param>
        public void SetQuestStage(DialogueOption option)
        {
            Quest q;

            if (option.IsQuestOption && option.AdvancesQuest)
            {
                q = QuestController.GetQuestForDialogue(option.QuestID);

                // Only advance the Quest if we are at the correct Stage
                if (q.Stage == option.QuestStage)
                {
                    if (passCheckQ)
                    {
                        // Check if we need to add any description to the Quest
                        if (option.AddQuestDesc != null && option.AddQuestDesc != " ")
                            q.AddDescription = option.AddQuestDesc;

                        q.SetStage(true, option.FailObjective);
                    }
                    else
                    {
                        passCheckQ = true;

                        if (!q.Description.Contains("(Speech Failed) I failed my attempt to persuade the Citizen to give up information about the General."))
                            q.AddDescription = "(Speech Failed) I failed my attempt to persuade the Citizen to give up information about the General. " +
                                "I should continue trying to persuade him.";
                    }
                }
            }
            
        }

        /// <summary>
        /// Checks if the Player will succeed or fail the Skill Check
        /// </summary>
        /// <param name="option"></param>
        public void CheckSkillRequirement(DialogueOption option)
        {
            Skill sk;
            float chance = 0.0f;
            float rand1, rand2;

            // Get the Skill we will use to determine how difficult passing will be
            if (option.SkillRequire == SkillCheck.SPEECH)
                sk = PlayerController.Pc.Player.Speech;
            else if (option.SkillRequire == SkillCheck.ARMOR)
                sk = PlayerController.Pc.Player.Armor;
            else if (option.SkillRequire == SkillCheck.ATHLETICS)
                sk = PlayerController.Pc.Player.Athletics;
            else if (option.SkillRequire == SkillCheck.BARTER)
                sk = PlayerController.Pc.Player.Barter;
            else if (option.SkillRequire == SkillCheck.SPELLS)
                sk = PlayerController.Pc.Player.SpellSkill;
            else if (option.SkillRequire == SkillCheck.SWORDS)
                sk = PlayerController.Pc.Player.Swords;
            else
                sk = PlayerController.Pc.Player.Unarmored;

            // Set the chance that we will pass this check
            chance = (float)((float)sk.Level / (float)option.SkillDifficulty);
            Debug.Log("Chance: " + chance);

            // Clamp the Speech Check Success Percentage between 2.5% and 100%
            if (chance > 1)
                chance = 1.0f;
            else if (chance < 0.025f)
                chance = 0.025f;

            // Get 2 random numbers between 0 and 1
            rand1 = Random.Range(0.0f, 1.0f);
            rand2 = Random.Range(0.0f, 1.0f);

            // If the difference between the two numbers is less than or equal to the chance
            // we have passed the skill check
            if (Mathf.Abs(rand1 - rand2) <= chance)
                passCheck = true;
            else
                passCheck = false;

            passCheckQ = passCheck;

            // Add to the Number of Speech Checks
            WorldController.Data.StatValue.SpeechChecks += 1;

            // Add to the Number of Speech Checks Passed or Failed
            if (passCheck)
                WorldController.Data.StatValue.SpeechPassed += 1;
            else
                WorldController.Data.StatValue.SpeechFailed += 1;

            PlayerController.Pc.Player.SpeechSuccess(chance, passCheck);
        }

        /// <summary>
        /// Called when the Option will start the Trading menu
        /// </summary>
        /// <param name="option"></param>
        public void StartMerchant(DialogueOption option)
        {
            // Set the Inventory Menu up for Trading
            InventoryMenu.IsTrade = true;
            InventoryMenu.SetOtherInv(npcC, option.FreeTrade);
            InventoryMenu.dmc = gameObject;
            openMerch = true;
            CanClose = false;
        }

        /// <summary>
        /// Called when we press an Option that is One Time Only
        /// </summary>
        /// <param name="option"></param>
        public void UsedOneTime(DialogueOption option)
        {
            option.UsedOneTime = true;
        }

        /// <summary>
        /// Called when we press an Option that ends the Dialogue
        /// </summary>
        /// <param name="option"></param>
        public IEnumerator EndsDialogue(DialogueOption option)
        {
            ending = true;
            yield return new WaitForSeconds(0.3f);
        }

    }

}

