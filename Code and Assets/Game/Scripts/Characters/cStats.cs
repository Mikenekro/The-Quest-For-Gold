using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// Holds all of the data about the CharStats script so we can Save and Load the important values
    /// </summary>
    [System.Serializable]
    public class CStats
    {
        [SerializeField]
        private int bodyPos;
        [SerializeField]
        private int helmPos;
        [SerializeField]
        private int hairPos;
        [SerializeField]
        private int shirtPos;
        [SerializeField]
        private int pantsPos;
        [SerializeField]
        private int shoesPos;
        [SerializeField]
        private int glovesPos;
        [SerializeField]
        private int weaponPos;

        // Base Colors selected in the Character Creation Menu
        public SaveColor BaseHairColor { get; set; }
        public SaveColor BaseShirtColor { get; set; }
        public SaveColor BasePantsColor { get; set; }
        public SaveColor BaseShoesColor { get; set; }

        // Colors for the Body Parts that will change color
        public SaveColor HelmColor { get; set; }
        public SaveColor HairColor { get; set; }
        public SaveColor ShirtColor { get; set; }
        public SaveColor PantsColor { get; set; }
        public SaveColor ShoesColor { get; set; }
        public SaveColor GlovesColor { get; set; }
        public SaveColor WeaponsColor { get; set; }

        public SaveVector3 MoveDir { get; set; }

        public bool IsMale { get; set; }
        public bool IsDead { get; set; }

        // Positions in Array where this character gets their Body Parts
        public int BodyPos { get { return bodyPos; } set { bodyPos = value; } }
        public int HelmPos { get { return helmPos; } set { helmPos = value; } }
        public int HairPos { get { return hairPos; } set { hairPos = value; } }
        public int ShirtPos { get { return shirtPos; } set { shirtPos = value; } }
        public int PantsPos { get { return pantsPos; } set { pantsPos = value; } }
        public int ShoesPos { get { return shoesPos; } set { shoesPos = value; } }
        public int GlovesPos { get { return glovesPos; } set { glovesPos = value; } }
        public int WeaponPos { get { return weaponPos; } set { weaponPos = value; } }

        public CStats()
        {
        }
    }

    /// <summary>
    /// Struct used to save Vectors as a single variable
    /// </summary>
    [System.Serializable]
    public struct SaveVector3
    {
        private float x1;
        private float y1;
        private float z1;
        public float X { get { return x1; } set { x1 = value; } }
        public float Y { get { return y1; } set { y1 = value; } }
        public float Z { get { return z1; } set { z1 = value; } }

        public SaveVector3(float _x, float _y, float _z)
        {
            x1 = _x;
            y1 = _y;
            z1 = _z;
        }
        public void Set(float _x, float _y, float _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }
    }

    /// <summary>
    /// Struct used to save colors as a single variable
    /// </summary>
    [System.Serializable]
    public struct SaveColor
    {
        private float _r;
        private float _g;
        private float _b;
        private float _a;

        /// <summary>
        /// Red color
        /// </summary>
        public float R
        {
            get { return _r; }
            set
            {
                if (value >= 0.0f && value <= 1.0f) _r = value;
                else if (value >= 1.0f) _r = 1.0f;
                else if (value <= 0.0f) _r = 0.0f;
            }
        }
        /// <summary>
        /// Green color
        /// </summary>
        public float G
        {
            get { return _g; }
            set
            {
                if (value >= 0.0f && value <= 1.0f) _g = value;
                else if (value >= 1.0f) _g = 1.0f;
                else if (value <= 0.0f) _g = 0.0f;
            }
        }
        /// <summary>
        /// Blue color
        /// </summary>
        public float B
        {
            get { return _b; }
            set
            {
                if (value >= 0.0f && value <= 1.0f) _b = value;
                else if (value >= 1.0f) _b = 1.0f;
                else if (value <= 0.0f) _b = 0.0f;
            }
        }
        /// <summary>
        /// Alpha field
        /// </summary>
        public float A
        {
            get { return _a; }
            set
            {
                if (value >= 0.0f && value <= 1.0f) _a = value;
                else if (value >= 1.0f) _a = 1.0f;
                else if (value <= 0.0f) _a = 0.0f;
            }
        }

        public SaveColor(float r1, float g1, float b1, float a1)
        {
            _r = r1;
            _g = g1;
            _b = b1;
            _a = a1;
        }
    }
}

