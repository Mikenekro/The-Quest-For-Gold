using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using schoolRPG;

// This editor script is used to pull all of the needed sprites into their correct inspector references based on the sprite sheet that we input

[CustomEditor(typeof(CharList))]
public class CharListEditor : Editor
{
    private int i;
    private int j;
    private int k;
    private int pulledFrom;
    private int addedTotal;
    private int addedNeeded;
    private bool remove;
    private CharList cl;

	// Use this for initialization
	void OnEnable()
    {
        cl = (CharList)target;
	}
	
	// Update is called once per frame
	public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        try
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Remove SpriteSheets After Pull?:");
            remove = EditorGUILayout.Toggle(" ", remove, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (Application.isEditor)
            {
                // Only pull the Sprites if we specify that we want to
                if (GUILayout.Button("Pull Sprites From SpriteSheet"))
                {
                    addedTotal = 0;
                    addedNeeded = 0;
                    pulledFrom = 0;
                    Debug.Log("Pulling Sprites...");
                    PullSprites();

                    Debug.Log("Pulled From: " + pulledFrom + " different Sprite Sheets");

                    if (addedTotal < addedNeeded)
                        Debug.LogWarning("Did not add all Sprites to CharList! Needed: " + addedNeeded.ToString() + " Sprites, Added: " + addedTotal.ToString() + " Sprites");
                    else if (addedTotal > addedNeeded)
                        Debug.LogWarning("Added more Sprites than needed! Needed: " + addedNeeded.ToString() + " Sprites, Added: " + addedTotal.ToString() + " Sprites");
                    else
                        Debug.Log("Success!: All Sprites have been added to the BodyParts List!");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Critical Error: " + ex.Message);
        }
        
    }

    /// <summary>
    /// Pull each Sprite into the CharList when a SpriteSheet is attached.
    /// </summary>
    public void PullSprites()
    {
        //path = "Assets/Game/Sprites/Characters/";
        //if (cl.body[i].name.Split('_')[0] == "Male")
        //    path += "Body/Male/";

        Debug.Log("Pulling Body Sprites, Total: " + cl.body.Count.ToString());
        for (i = 0; i < cl.body.Count; ++i)
        {
            if (cl.body[i].spriteSheet != null)
            {
                PullFrom(cl.body[i], false);
            }
        }
        Debug.Log("Pulling Helmet Sprites, Total: " + cl.helmet.Count.ToString());
        for (i = 0; i < cl.helmet.Count; ++i)
        {
            if (cl.helmet[i].spriteSheet != null)
            {
                PullFrom(cl.helmet[i], false);
            }
        }
        Debug.Log("Pulling Hair Sprites, Total: " + cl.hair.Count.ToString());
        for (i = 0; i < cl.hair.Count; ++i)
        {
            if (cl.hair[i].spriteSheet != null)
            {
                PullFrom(cl.hair[i], false);
            }
        }
        Debug.Log("Pulling Shirt Sprites, Total: " + cl.shirt.Count.ToString());
        for (i = 0; i < cl.shirt.Count; ++i)
        {
            if (cl.shirt[i].spriteSheet != null)
            {

                PullFrom(cl.shirt[i], false);
            }
        }
        Debug.Log("Pulling Pants Sprites, Total: " + cl.pants.Count.ToString());
        for (i = 0; i < cl.pants.Count; ++i)
        {
            if (cl.pants[i].spriteSheet != null)
            {
                PullFrom(cl.pants[i], false);
            }
        }
        Debug.Log("Pulling Shoes Sprites, Total: " + cl.shoes.Count.ToString());
        for (i = 0; i < cl.shoes.Count; ++i)
        {
            if (cl.shoes[i].spriteSheet != null)
            {
                PullFrom(cl.shoes[i], false);
            }
        }
        Debug.Log("Pulling Gloves Sprites, Total: " + cl.gloves.Count.ToString());
        for (i = 0; i < cl.gloves.Count; ++i)
        {
            if (cl.gloves[i].spriteSheet != null)
            {
                PullFrom(cl.gloves[i], false);
            }
        }
        Debug.Log("Pulling Weapons Sprites, Total: " + cl.weapons.Count.ToString());
        for (i = 0; i < cl.weapons.Count; ++i)
        {
            if (cl.weapons[i].spriteSheet != null)
            {
                PullFrom(cl.weapons[i], true);
            }
        }
        Debug.Log("Pulling NPC Sprites, Total: " + cl.npc.Count.ToString());
        for (i = 0; i < cl.npc.Count; ++i)
        {
            if (cl.npc[i].spriteSheet != null)
            {
                PullFrom(cl.npc[i], false);
            }
        }
    }

    /// <summary>
    /// Pulls the Sprites from the inserted Texture
    /// </summary>
    public void PullFrom(CharParts parts, bool wep)
    {
        int prev;
        int store = 0;
        Sprite temp;
        string spriteSheet = AssetDatabase.GetAssetPath(parts.spriteSheet);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
        

        prev = addedTotal;

        for (j = 0; j < sprites.Length; ++j)
        {
            if (wep)
            {
                // Attack Up
                if (j <= 5)
                {
                    if (j == 0)
                        k = 0;

                    PullInto(parts.AttackUp, sprites[j]);
                }
                // Attack Left
                else if (j <= 11)
                {
                    if (j == 6)
                        k = 0;

                    PullInto(parts.AttackLeft, sprites[j]);
                }
                // Attack Down
                else if (j <= 17)
                {
                    if (j == 12)
                        k = 0;

                    PullInto(parts.AttackDown, sprites[j]);
                }
                // Attack Right
                else if (j <= 23)
                {
                    if (j == 18)
                        k = 0;

                    PullInto(parts.AttackRight, sprites[j]);
                }
            }
            else if (!wep)
            {
                // Magic Up
                if (j >= 0 && j <= 6)
                {
                    // Reset the CharParts List Value
                    if (j == 0)
                        k = 0;

                    PullInto(parts.MagicUp, sprites[j]);
                }
                // Magic Left
                else if (j >= 13 && j <= 19)
                {
                    // Reset the CharParts List Value
                    if (j == 13)
                        k = 0;

                    PullInto(parts.MagicLeft, sprites[j]);
                    
                }
                // Magic Down
                else if (j >= 26 && j <= 32)
                {
                    // Reset the CharParts List Value
                    if (j == 26)
                        k = 0;

                    PullInto(parts.MagicDown, sprites[j]);
                    
                }
                // Magic Right
                else if (j >= 39 && j <= 45)
                {
                    // Reset the CharParts List Value
                    if (j == 39)
                        k = 0;

                    PullInto(parts.MagicRight, sprites[j]);
                    
                }
                // Walk Up
                else if (j >= 104 && j <= 112)
                {
                    // Reset the CharParts List Value
                    if (j == 104)
                    {
                        k = 0;
                        PullInto(parts.IdleUp, sprites[j]);
                        k = 0;
                    }
                    else if (j == 105)
                    {
                        store = k;
                        k = 1;
                        PullInto(parts.IdleUp, sprites[j]);
                        PullInto(parts.IdleUp, sprites[104]);
                        k = store;
                    }

                    PullInto(parts.WalkUp, sprites[j]);
                    
                }
                // Walk Left
                else if (j >= 117 && j <= 125)
                {
                    // Reset the CharParts List Value
                    if (j == 117)
                    {
                        k = 0;
                        PullInto(parts.IdleLeft, sprites[j]);
                        k = 0;
                    }
                    else if (j == 125)
                    {
                        store = k;
                        k = 1;
                        PullInto(parts.IdleLeft, sprites[j]);
                        PullInto(parts.IdleLeft, sprites[117]);
                        k = store;
                    }

                    PullInto(parts.WalkLeft, sprites[j]);
                    
                }
                // Walk Down
                else if (j >= 130 && j <= 138)
                {
                    // Reset the CharParts List Value
                    if (j == 130)
                    {
                        k = 0;
                        PullInto(parts.IdleDown, sprites[j]);
                        k = 0;
                    }
                    else if (j == 131)
                    {
                        store = k;
                        k = 1;
                        PullInto(parts.IdleDown, sprites[j]);
                        PullInto(parts.IdleDown, sprites[130]);
                        k = store;
                    }

                    PullInto(parts.WalkDown, sprites[j]);
                    
                }
                // Walk Right
                else if (j >= 143 && j <= 151)
                {
                    // Reset the CharParts List Value
                    if (j == 143)
                    {
                        k = 0;
                        PullInto(parts.IdleRight, sprites[j]);
                        k = 0;
                    }
                    else if (j == 151)
                    {
                        store = k;
                        k = 1;
                        PullInto(parts.IdleRight, sprites[j]);
                        PullInto(parts.IdleRight, sprites[143]);
                        k = store;
                    }

                    PullInto(parts.WalkRight, sprites[j]);
                    
                }
                // Attack Up
                else if (j >= 156 && j <= 161)
                {
                    // Reset the CharParts List Value
                    if (j == 156)
                        k = 0;

                    PullInto(parts.AttackUp, sprites[j]);
                    
                }
                // Attack Left
                else if (j >= 169 && j <= 174)
                {
                    // Reset the CharParts List Value
                    if (j == 169)
                        k = 0;

                    PullInto(parts.AttackLeft, sprites[j]);
                    
                }
                // Attack Down
                else if (j >= 182 && j <= 187)
                {
                    // Reset the CharParts List Value
                    if (j == 182)
                        k = 0;

                    PullInto(parts.AttackDown, sprites[j]);
                    
                }
                // Attack Right
                else if (j >= 195 && j <= 200)
                {
                    // Reset the CharParts List Value
                    if (j == 195)
                        k = 0;

                    PullInto(parts.AttackRight, sprites[j]);
                    
                }
                // Death
                else if (j >= 260 && j <= 265)
                {
                    // Reset the CharParts List Value
                    if (j == 260)
                        k = 0;

                    PullInto(parts.Death, sprites[j]);
                }
            } // End if(Wep)
        } // End For j = 0 to sprite.length


        if (wep)
        {
            pulledFrom += 1;
            addedNeeded += 24;

            // If we added the correct number of sprites and we want to remove the sprite sheet
            if ((prev + 24) == addedTotal && remove)
            {
                Debug.Log("Removed: " + parts.spriteSheet.name);
                parts.spriteSheet = null;
            }
        }
        else
        {
            pulledFrom += 1;
            addedNeeded += 106;

            // If we added the correct number of sprites and we want to remove the sprite sheet
            if ((prev + 106) == addedTotal && remove)
            {
                Debug.Log("Removed: " + parts.spriteSheet.name);
                parts.spriteSheet = null;
            }
        }
    } // End PullFrom()

    /// <summary>
    /// Pulls the Sprite into the List (Reset on every new list)
    /// </summary>
    /// <param name="list"></param>
    /// <param name="sprite"></param>
    public void PullInto(List<Sprite> list, Sprite sprite)
    {
        if (list.Count < (k + 1))
        {
            list.Add(sprite);
            addedTotal += 1;
        }
        else
        {
            // Make sure they are the same sprite
            if (list[k].name == sprite.name)
            {
                list[k] = sprite;
                addedTotal += 1;
            }
        }

        ++k;
    }
}
