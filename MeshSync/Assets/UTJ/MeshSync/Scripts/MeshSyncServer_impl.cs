using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJ
{
    public partial class MeshSyncServer
    {
        #region internal
        public enum MessageType
        {
            Unknown,
            Get,
            Set,
            Delete,
            Fence,
            Text,
            Screenshot,
        }

        public struct GetFlags
        {
            public int flags;
            public bool getTransform { get { return (flags & (1 << 0)) != 0; } }
            public bool getPoints { get { return (flags & (1 << 1)) != 0; } }
            public bool getNormals { get { return (flags & (1 << 2)) != 0; } }
            public bool getTangents { get { return (flags & (1 << 3)) != 0; } }
            public bool getUV { get { return (flags & (1 << 4)) != 0; } }
            public bool getIndices { get { return (flags & (1 << 5)) != 0; } }
            public bool getMaterialIDs { get { return (flags & (1 << 6)) != 0; } }
            public bool getBones { get { return (flags & (1 << 7)) != 0; } }
        }

        public struct GetMessage
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern GetFlags msGetGetFlags(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msGetGetBakeSkin(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msGetGetBakeCloth(IntPtr _this);

            public static explicit operator GetMessage(IntPtr v)
            {
                GetMessage ret;
                ret._this = v;
                return ret;
            }

            public GetFlags flags { get { return msGetGetFlags(_this); } }
            public bool bakeSkin { get { return msGetGetBakeSkin(_this) != 0; } }
            public bool bakeCloth { get { return msGetGetBakeCloth(_this) != 0; } }
        }

        public struct SetMessage
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern SceneData msSetGetSceneData(IntPtr _this);

            public static explicit operator SetMessage(IntPtr v)
            {
                SetMessage ret;
                ret._this = v;
                return ret;
            }

            public SceneData scene
            {
                get { return msSetGetSceneData(_this); }
            }
        }


        public struct DeleteMessage
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern int msDeleteGetNumTargets(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern IntPtr msDeleteGetPath(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern int msDeleteGetID(IntPtr _this, int i);

            public static explicit operator DeleteMessage(IntPtr v)
            {
                DeleteMessage ret;
                ret._this = v;
                return ret;
            }

            public int numTargets { get { return msDeleteGetNumTargets(_this); } }
            public string GetPath(int i) { return S(msDeleteGetPath(_this, i)); }
            public int GetID(int i) { return msDeleteGetID(_this, i); }
        }
        

        public struct FenceMessage
        {
            public enum FenceType
            {
                Unknown,
                SceneBegin,
                SceneEnd,
            }

            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern FenceType msFenceGetType(IntPtr _this);

            public static explicit operator FenceMessage(IntPtr v)
            {
                FenceMessage ret;
                ret._this = v;
                return ret;
            }

            public FenceType type { get { return msFenceGetType(_this); } }
        }


        public struct TextMessage
        {
            public enum TextType
            {
                Normal,
                Warning,
                Error,
            }

            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern IntPtr msTextGetText(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern TextType msTextGetType(IntPtr _this);

            public static explicit operator TextMessage(IntPtr v)
            {
                TextMessage ret;
                ret._this = v;
                return ret;
            }

            public string text { get { return S(msTextGetText(_this)); } }
            public TextType textType { get { return msTextGetType(_this); } }

            public void Print()
            {
                switch (textType) {
                    case TextType.Error:
                        Debug.LogError(text);
                        break;
                    case TextType.Warning:
                        Debug.LogWarning(text);
                        break;
                    default:
                        Debug.Log(text);
                        break;
                }

            }
        }



        public struct MeshDataFlags
        {
            public int flags;
            public bool visible
            {
                get { return (flags & (1 << 0)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 0)); }
            }
            public bool hasRefineSettings
            {
                get { return (flags & (1 << 1)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 1)); }
            }
            public bool hasIndices
            {
                get { return (flags & (1 << 2)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 2)); }
            }
            public bool hasCounts
            {
                get { return (flags & (1 << 3)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 3)); }
            }
            public bool hasPoints
            {
                get { return (flags & (1 << 4)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 4)); }
            }
            public bool hasNormals
            {
                get { return (flags & (1 << 5)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 5)); }
            }
            public bool hasTangents
            {
                get { return (flags & (1 << 6)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 6)); }
            }
            public bool hasUV
            {
                get { return (flags & (1 << 7)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 7)); }
            }
            public bool hasMaterialIDs
            {
                get { return (flags & (1 << 8)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 8)); }
            }
            public bool hasBones
            {
                get { return (flags & (1 << 9)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 9)); }
            }
            public bool hasNPoints
            {
                get { return (flags & (1 << 10)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 10)); }
            }
            public bool applyTRS
            {
                get { return (flags & (1 << 11)) != 0; }
                set { SwitchBits(ref flags, value, (1 << 11)); }
            }
        };

        public struct TRS
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 rotation_eularZXY;
            public Vector3 scale;
        };

        public struct MaterialData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern int msMaterialGetID(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMaterialSetID(IntPtr _this, int v);
            [DllImport("MeshSyncServer")] static extern IntPtr msMaterialGetName(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMaterialSetName(IntPtr _this, string v);
            [DllImport("MeshSyncServer")] static extern Color msMaterialGetColor(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMaterialSetColor(IntPtr _this, ref Color v);

            public int id {
                get { return msMaterialGetID(_this); }
                set { msMaterialSetID(_this, value); }
            }
            public string name {
                get { return S(msMaterialGetName(_this)); }
                set { msMaterialSetName(_this, value); }
            }
            public Color color {
                get { return msMaterialGetColor(_this); }
                set { msMaterialSetColor(_this, ref value); }
            }
        }

        public struct KeyframeData
        {
            internal IntPtr _this;

            [DllImport("MeshSyncServer")] static extern float msKeyframeGetTime(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern float msKeyframeGetValue(IntPtr _this);
            //[DllImport("MeshSyncServer")] static extern Vector2 msKeyframeGetInTangent(IntPtr _this);
            //[DllImport("MeshSyncServer")] static extern Vector2 msKeyframeGetOutTangent(IntPtr _this);

            public float time
            {
                get { return msKeyframeGetTime(_this); }
            }
            public float value
            {
                get { return msKeyframeGetValue(_this); }
            }
            //public Vector2 inTangent
            //{
            //    get { return msKeyframeGetInTangent(_this); }
            //}
            //public Vector2 outTangent
            //{
            //    get { return msKeyframeGetOutTangent(_this); }
            //}
        }

        public struct AnimationData
        {
            internal IntPtr _this;

            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumTranslationXKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumTranslationYKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumTranslationZKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetTranslationXKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetTranslationYKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetTranslationZKey(IntPtr _this, int i);

            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumRotationXKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumRotationYKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumRotationZKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetRotationXKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetRotationYKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetRotationZKey(IntPtr _this, int i);

            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumScaleXKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumScaleYKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumScaleZKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetScaleXKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetScaleYKey(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetScaleZKey(IntPtr _this, int i);

            [DllImport("MeshSyncServer")] static extern int msAnimationGetNumVisibilityKeys(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern KeyframeData msAnimationGetVisibilityKey(IntPtr _this, int i);

            public static explicit operator AnimationData(IntPtr v)
            {
                AnimationData ret;
                ret._this = v;
                return ret;
            }
            public static implicit operator bool(AnimationData v)
            {
                return v._this != IntPtr.Zero;
            }

            public KeyframeData[] translateX
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumTranslationXKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetTranslationXKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] translateY
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumTranslationYKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetTranslationYKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] translateZ
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumTranslationZKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetTranslationZKey(_this, i); }
                    return ret;
                }
            }

            public KeyframeData[] rotationX
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumRotationXKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetRotationXKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] rotationY
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumRotationYKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetRotationYKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] rotationZ
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumRotationZKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetRotationZKey(_this, i); }
                    return ret;
                }
            }

            public KeyframeData[] scaleX
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumScaleXKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetScaleXKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] scaleY
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumScaleYKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetScaleYKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] scaleZ
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumScaleZKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetScaleZKey(_this, i); }
                    return ret;
                }
            }
            public KeyframeData[] visibility
            {
                get
                {
                    var ret = new KeyframeData[msAnimationGetNumVisibilityKeys(_this)];
                    for (int i = 0; i < ret.Length; ++i) { ret[i] = msAnimationGetVisibilityKey(_this, i); }
                    return ret;
                }
            }

            public static AnimationCurve ToAnimatinCurve(KeyframeData[] src)
            {
                var ret = new AnimationCurve();
                foreach(var k in src)
                {
                    var i = ret.AddKey(new Keyframe(k.time, k.value));
                }
                return ret;
            }
        }


        public struct TransformData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern TransformData msTransformCreate();
            [DllImport("MeshSyncServer")] static extern int msTransformGetID(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msTransformSetID(IntPtr _this, int v);
            [DllImport("MeshSyncServer")] static extern int msTransformGetIndex(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msTransformSetIndex(IntPtr _this, int v);
            [DllImport("MeshSyncServer")] static extern IntPtr msTransformGetPath(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msTransformSetPath(IntPtr _this, string v);
            [DllImport("MeshSyncServer")] static extern void msTransformGetTRS(IntPtr _this, ref TRS dst);
            [DllImport("MeshSyncServer")] static extern void msTransformSetTRS(IntPtr _this, ref TRS v);
            [DllImport("MeshSyncServer")] static extern IntPtr msTransformGetReference(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msTransformSetReference(IntPtr _this, string v);
            [DllImport("MeshSyncServer")] static extern AnimationData msTransformGetAnimation(IntPtr _this);

            public static explicit operator TransformData(IntPtr v)
            {
                TransformData ret;
                ret._this = v;
                return ret;
            }

            public static TransformData Create()
            {
                return msTransformCreate();
            }

            public int id
            {
                get { return msTransformGetID(_this); }
                set { msTransformSetID(_this, value); }
            }
            public int index
            {
                get { return msTransformGetIndex(_this); }
                set { msTransformSetIndex(_this, value); }
            }
            public string path
            {
                get { return S(msTransformGetPath(_this)); }
                set { msTransformSetPath(_this, value); }
            }

            public TRS trs
            {
                get
                {
                    var ret = default(TRS);
                    msTransformGetTRS(_this, ref ret);
                    return ret;
                }
                set
                {
                    msTransformSetTRS(_this, ref value);
                }
            }

            public string reference
            {
                get { return S(msTransformGetReference(_this)); }
                set { msTransformSetReference(_this, value); }
            }

            public AnimationData animation
            {
                get { return msTransformGetAnimation(_this); }
            }
        }

        public struct CameraData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern CameraData msCameraCreate();
            [DllImport("MeshSyncServer")] static extern float msCameraGetFov(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msCameraSetFov(IntPtr _this, float v);

            public static explicit operator CameraData(IntPtr v)
            {
                CameraData ret;
                ret._this = v;
                return ret;
            }

            public static CameraData Create()
            {
                return msCameraCreate();
            }

            public TransformData transform
            {
                get { return (TransformData)_this; }
            }

            public float fov
            {
                get { return msCameraGetFov(_this); }
                set { msCameraSetFov(_this, value); }
            }
        }

        public struct MeshData
        {
            internal IntPtr _this;

            [DllImport("MeshSyncServer")] static extern MeshData msMeshCreate();
            [DllImport("MeshSyncServer")] static extern MeshDataFlags msMeshGetFlags(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMeshSetFlags(IntPtr _this, MeshDataFlags v);
            [DllImport("MeshSyncServer")] static extern int msMeshGetNumPoints(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msMeshGetNumIndices(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msMeshGetNumSplits(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMeshReadPoints(IntPtr _this, Vector3[] dst);
            [DllImport("MeshSyncServer")] static extern void msMeshWritePoints(IntPtr _this, Vector3[] v, int size);
            [DllImport("MeshSyncServer")] static extern void msMeshReadNormals(IntPtr _this, Vector3[] dst);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteNormals(IntPtr _this, Vector3[] v, int size);
            [DllImport("MeshSyncServer")] static extern void msMeshReadTangents(IntPtr _this, Vector4[] dst);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteTangents(IntPtr _this, Vector4[] v, int size);
            [DllImport("MeshSyncServer")] static extern void msMeshReadUV(IntPtr _this, Vector2[] dst);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteUV(IntPtr _this, Vector2[] v, int size);
            [DllImport("MeshSyncServer")] static extern void msMeshReadIndices(IntPtr _this, int[] dst);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteIndices(IntPtr _this, int[] v, int size);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteSubmeshTriangles(IntPtr _this, int[] v, int size, int materialID);

            [DllImport("MeshSyncServer")] static extern int msMeshGetNumWeights4(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msMeshReadWeights4(IntPtr _this, BoneWeight[] weights);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteWeights4(IntPtr _this, BoneWeight[] weights, int size);
            [DllImport("MeshSyncServer")] static extern int msMeshGetNumBones(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern IntPtr msMeshGetBonePath(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern void msMeshSetBonePath(IntPtr _this, string v, int i);
            [DllImport("MeshSyncServer")] static extern void msMeshReadBindPoses(IntPtr _this, Matrix4x4[] v);
            [DllImport("MeshSyncServer")] static extern void msMeshWriteBindPoses(IntPtr _this, Matrix4x4[] v, int size);

            [DllImport("MeshSyncServer")] static extern void msMeshSetLocal2World(IntPtr _this, ref Matrix4x4 v);
            [DllImport("MeshSyncServer")] static extern void msMeshSetWorld2Local(IntPtr _this, ref Matrix4x4 v);

            [DllImport("MeshSyncServer")] static extern SplitData msMeshGetSplit(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern int msMeshGetNumSubmeshes(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern SubmeshData msMeshGetSubmesh(IntPtr _this, int i);

            public static MeshData Create()
            {
                return msMeshCreate();
            }

            public static explicit operator MeshData(IntPtr v)
            {
                MeshData ret;
                ret._this = v;
                return ret;
            }

            public TransformData transform
            {
                get { return (TransformData)_this; }
            }

            public MeshDataFlags flags
            {
                get { return msMeshGetFlags(_this); }
                set { msMeshSetFlags(_this, value); }
            }

            public int numPoints { get { return msMeshGetNumPoints(_this); } }
            public int numIndices { get { return msMeshGetNumIndices(_this); } }
            public int numSplits { get { return msMeshGetNumSplits(_this); } }
            public Vector3[] points
            {
                get
                {
                    var ret = new Vector3[numPoints];
                    msMeshReadPoints(_this, ret);
                    return ret;
                }
                set
                {
                    msMeshWritePoints(_this, value, value.Length);
                }
            }
            public Vector3[] normals
            {
                get
                {
                    var ret = new Vector3[numPoints];
                    msMeshReadNormals(_this, ret);
                    return ret;
                }
                set
                {
                    msMeshWriteNormals(_this, value, value.Length);
                }
            }
            public Vector4[] tangents
            {
                get
                {
                    var ret = new Vector4[numPoints];
                    msMeshReadTangents(_this, ret);
                    return ret;
                }
                set
                {
                    msMeshWriteTangents(_this, value, value.Length);
                }
            }
            public Vector2[] uv
            {
                get
                {
                    var ret = new Vector2[numPoints];
                    msMeshReadUV(_this, ret);
                    return ret;
                }
                set
                {
                    msMeshWriteUV(_this, value, value.Length);
                }
            }
            public int[] indices
            {
                get
                {
                    var ret = new int[numIndices];
                    msMeshReadIndices(_this, ret);
                    return ret;
                }
                set
                {
                    msMeshWriteIndices(_this, value, value.Length);
                }
            }

            public Matrix4x4 local2world { set { msMeshSetLocal2World(_this, ref value); } }
            public Matrix4x4 world2local { set { msMeshSetWorld2Local(_this, ref value); } }

            public SplitData GetSplit(int i)
            {
                return msMeshGetSplit(_this, i);
            }
            public void WriteSubmeshTriangles(int[] indices, int materialID)
            {
                msMeshWriteSubmeshTriangles(_this, indices, indices.Length, materialID);
            }

            public int numBoneWeights
            {
                get { return msMeshGetNumWeights4(_this); }
            }
            public BoneWeight[] boneWeights
            {
                get {
                    var ret = new BoneWeight[numBoneWeights];
                    msMeshReadWeights4(_this, ret);
                    return ret;
                }
                set { msMeshWriteWeights4(_this, value, value.Length); }
            }

            public int numBones
            {
                get { return msMeshGetNumBones(_this); }
            }
            public Matrix4x4[] bindposes
            {
                get
                {
                    var ret = new Matrix4x4[numBones];
                    msMeshReadBindPoses(_this, ret);
                    return ret;
                }
                set { msMeshWriteBindPoses(_this, value, value.Length); }
            }
            public void SetBonePaths(Transform[] bones)
            {
                int n = bones.Length;
                for (int i = 0; i < n; ++i)
                {
                    string path = BuildPath(bones[i]);
                    msMeshSetBonePath(_this, path, i);
                }
            }
            public string[] GetBonePaths()
            {
                int n = numBones;
                var ret = new string[n];
                for (int i = 0; i < n; ++i)
                {
                    ret[i] = S(msMeshGetBonePath(_this, i));
                }
                return ret;
            }

            public int numSubmeshes { get { return msMeshGetNumSubmeshes(_this); } }
            public SubmeshData GetSubmesh(int i)
            {
                return msMeshGetSubmesh(_this, i);
            }
        };

        public struct SplitData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern int msSplitGetNumPoints(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msSplitGetNumIndices(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msSplitGetNumSubmeshes(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msSplitReadPoints(IntPtr _this, Vector3[] dst);
            [DllImport("MeshSyncServer")] static extern void msSplitReadNormals(IntPtr _this, Vector3[] dst);
            [DllImport("MeshSyncServer")] static extern void msSplitReadTangents(IntPtr _this, Vector4[] dst);
            [DllImport("MeshSyncServer")] static extern void msSplitReadUV(IntPtr _this, Vector2[] dst);
            [DllImport("MeshSyncServer")] static extern void msSplitReadWeights4(IntPtr _this, BoneWeight[] dst);
            [DllImport("MeshSyncServer")] static extern void msSplitReadIndices(IntPtr _this, int[] dst);
            [DllImport("MeshSyncServer")] static extern SubmeshData msSplitGetSubmesh(IntPtr _this, int i);

            public int numPoints { get { return msSplitGetNumPoints(_this); } }
            public int numIndices { get { return msSplitGetNumIndices(_this); } }
            public int numSubmeshes { get { return msSplitGetNumSubmeshes(_this); } }
            public Vector3[] points
            {
                get
                {
                    var ret = new Vector3[numPoints];
                    msSplitReadPoints(_this, ret);
                    return ret;
                }
            }
            public Vector3[] normals
            {
                get
                {
                    var ret = new Vector3[numPoints];
                    msSplitReadNormals(_this, ret);
                    return ret;
                }
            }
            public Vector4[] tangents
            {
                get
                {
                    var ret = new Vector4[numPoints];
                    msSplitReadTangents(_this, ret);
                    return ret;
                }
            }
            public Vector2[] uv
            {
                get
                {
                    var ret = new Vector2[numPoints];
                    msSplitReadUV(_this, ret);
                    return ret;
                }
            }
            public BoneWeight[] boneWeights
            {
                get
                {
                    var ret = new BoneWeight[numPoints];
                    msSplitReadWeights4(_this, ret);
                    return ret;
                }
            }
            public int[] indices
            {
                get
                {
                    var ret = new int[numIndices];
                    msSplitReadIndices(_this, ret);
                    return ret;
                }
            }
            public SubmeshData GetSubmesh(int i)
            {
                return msSplitGetSubmesh(_this, i);
            }
        }

        public struct SubmeshData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern int msSubmeshGetNumIndices(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern int msSubmeshGetMaterialID(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern void msSubmeshReadIndices(IntPtr _this, int[] dst);

            public int numIndices { get { return msSubmeshGetNumIndices(_this); } }
            public int materialID { get { return msSubmeshGetMaterialID(_this); } }
            public int[] indices
            {
                get
                {
                    var ret = new int[numIndices];
                    msSubmeshReadIndices(_this, ret);
                    return ret;
                }
            }
        }

        public struct SceneData
        {
            internal IntPtr _this;
            [DllImport("MeshSyncServer")] static extern int msSceneGetNumMeshes(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern MeshData msSceneGetMeshData(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern int msSceneGetNumTransforms(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern TransformData msSceneGetTransformData(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern int msSceneGetNumCameras(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern CameraData msSceneGetCameraData(IntPtr _this, int i);
            [DllImport("MeshSyncServer")] static extern int msSceneGetNumMaterials(IntPtr _this);
            [DllImport("MeshSyncServer")] static extern MaterialData msSceneGetMaterialData(IntPtr _this, int i);

            public int numMeshes { get { return msSceneGetNumMeshes(_this); } }
            public int numTransforms { get { return msSceneGetNumTransforms(_this); } }
            public int numCameras { get { return msSceneGetNumCameras(_this); } }
            public int numMaterials { get { return msSceneGetNumMaterials(_this); } }

            public MeshData GetMesh(int i) { return msSceneGetMeshData(_this, i); }
            public TransformData GetTransform(int i) { return msSceneGetTransformData(_this, i); }
            public CameraData GetCamera(int i) { return msSceneGetCameraData(_this, i); }
            public MaterialData GetMaterial(int i) { return msSceneGetMaterialData(_this, i); }
        }



        public struct ServerSettings
        {
            public int max_queue;
            public int max_threads;
            public ushort port;

            public static ServerSettings default_value
            {
                get
                {
                    return new ServerSettings
                    {
                        max_queue = 256,
                        max_threads = 8,
                        port = 8080,
                    };
                }
            }
        }

        [DllImport("MeshSyncServer")] static extern IntPtr msServerStart(ref ServerSettings settings);
        [DllImport("MeshSyncServer")] static extern void msServerStop(IntPtr sv);

        delegate void msMessageHandler(MessageType type, IntPtr data);
        [DllImport("MeshSyncServer")] static extern int msServerProcessMessages(IntPtr sv, msMessageHandler handler);

        [DllImport("MeshSyncServer")] static extern void msServerBeginServe(IntPtr sv);
        [DllImport("MeshSyncServer")] static extern void msServerEndServe(IntPtr sv);
        [DllImport("MeshSyncServer")] static extern void msServerServeMesh(IntPtr sv, MeshData data);
        [DllImport("MeshSyncServer")] static extern void msServerSetNumMaterials(IntPtr sv, int n);
        [DllImport("MeshSyncServer")] static extern void msServerSetScreenshotFilePath(IntPtr sv, string path);

        static void SwitchBits(ref int flags, bool f, int bit)
        {

            if (f) { flags |= bit; }
            else { flags &= ~bit; }
        }

        public static IntPtr RawPtr(Array v)
        {
            return v == null ? IntPtr.Zero : Marshal.UnsafeAddrOfPinnedArrayElement(v, 0);
        }
        public static string S(IntPtr cstring)
        {
            return cstring == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(cstring);
        }

        [Serializable]
        public class Record
        {
            public int index;
            public GameObject go;
            public Mesh origMesh;
            public Mesh editMesh;
            public int[] materialIDs = new int[0];
            public int[] submeshCounts = new int[0];
            public bool recved = false;

            // return true if modified
            public bool BuildMaterialData(MeshData md)
            {
                int num_submeshes = md.numSubmeshes;
                if(num_submeshes == 0) { return false; }

                var mids = new int[num_submeshes];
                for (int i = 0; i < num_submeshes; ++i)
                {
                    mids[i] = md.GetSubmesh(i).materialID;
                }

                int num_splits = md.numSplits;
                var scs = new int[num_splits];
                for (int i = 0; i < num_splits; ++i)
                {
                    scs[i] = md.GetSplit(i).numSubmeshes;
                }

                bool ret = !materialIDs.SequenceEqual(mids) || !submeshCounts.SequenceEqual(scs);
                materialIDs = mids;
                submeshCounts = scs;
                return ret;
            }

            public int maxMaterialID
            {
                get
                {
                    return materialIDs.Length > 0 ? materialIDs.Max() : 0;
                }
            }
        }

        [Serializable]
        public class MaterialHolder
        {
            public int id;
            public string name;
            public Color color = Color.white;
            public Material material;
        }

        #endregion
    }
}