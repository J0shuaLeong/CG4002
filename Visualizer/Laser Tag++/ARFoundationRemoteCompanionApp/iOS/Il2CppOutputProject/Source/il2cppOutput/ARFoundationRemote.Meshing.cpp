#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename R>
struct GenericVirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_virtual_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
struct InterfaceActionInvoker0
{
	typedef void (*Action)(void*, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, invokeData.method);
	}
};

struct HashSet_1_t174593AE6599738C19A33586587D63534CED9F0F;
struct List_1_tA1A04BD6B1EE83992AE369D5DB31A028E9B57822;
struct ISubsystemDescriptor_tEF29944D579CC7D70F52CB883150735991D54E6E;
struct IXRMeshSubsystem_tF44E5BE9DA1195842896DEE4AC7FFA6E77AFCA37;
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C;
struct String_t;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE;
struct XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF;
struct XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976;
struct XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52;
struct XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2;
struct XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C;
struct _Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4;
struct _Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3;

IL2CPP_EXTERN_C RuntimeClass* ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* _Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* _Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C const RuntimeMethod* XRLoader_GetLoadedSubsystem_TisXRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2_m081FBFB83316621ED3D69C9D27376B59CB646519_RuntimeMethod_var;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_t9BDE8315E0C23AF0DD70CA11994D9E6BE9F520E5 
{
};
struct Extensions_tEABED90A3DBD793302E985B155CE654DA227B896  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF  : public RuntimeObject
{
	_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4* ___Manager;
};
struct XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C  : public RuntimeObject
{
};
struct _Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4  : public RuntimeObject
{
	_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3* ___activeLoader;
};
struct _Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3  : public RuntimeObject
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct IntegratedSubsystem_t990160A89854D87C0836DC589B720231C02D4CE3  : public RuntimeObject
{
	intptr_t ___m_Ptr;
	RuntimeObject* ___m_SubsystemDescriptor;
};
struct IntegratedSubsystem_t990160A89854D87C0836DC589B720231C02D4CE3_marshaled_pinvoke
{
	intptr_t ___m_Ptr;
	RuntimeObject* ___m_SubsystemDescriptor;
};
struct IntegratedSubsystem_t990160A89854D87C0836DC589B720231C02D4CE3_marshaled_com
{
	intptr_t ___m_Ptr;
	RuntimeObject* ___m_SubsystemDescriptor;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C  : public RuntimeObject
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
	intptr_t ___m_CachedPtr;
};
struct IntegratedSubsystem_1_t3AB3A9BAB10A4D1420E63EEA50D40FEE65F41C03  : public IntegratedSubsystem_t990160A89854D87C0836DC589B720231C02D4CE3
{
};
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A_marshaled_pinvoke : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
};
struct ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A_marshaled_com : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
};
struct XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
	XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* ___m_LoaderManagerInstance;
	bool ___m_InitManagerOnStart;
	XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* ___m_XRManager;
	bool ___m_ProviderIntialized;
	bool ___m_ProviderStarted;
};
struct XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
};
struct XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52  : public ScriptableObject_tB3BFDB921A1B1795B38A5417D3B97A89A140436A
{
	bool ___m_InitializationComplete;
	bool ___m_RequiresSettingsUpdate;
	bool ___m_AutomaticLoading;
	bool ___m_AutomaticRunning;
	List_1_tA1A04BD6B1EE83992AE369D5DB31A028E9B57822* ___m_Loaders;
	HashSet_1_t174593AE6599738C19A33586587D63534CED9F0F* ___m_RegisteredLoaders;
	XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* ___U3CactiveLoaderU3Ek__BackingField;
};
struct XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2  : public IntegratedSubsystem_1_t3AB3A9BAB10A4D1420E63EEA50D40FEE65F41C03
{
};
struct XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_StaticFields
{
	XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF* ___Instance;
};
struct XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_StaticFields
{
	RuntimeObject* ___subsystemDelegate;
};
struct _Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_StaticFields
{
	RuntimeObject* ___subsystem;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_StaticFields
{
	int32_t ___OffsetOfInstanceIDInCPlusPlusObject;
};
struct XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE_StaticFields
{
	String_t* ___k_SettingsKey;
	XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* ___s_RuntimeSettingsInstance;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool XRManagerSettings_get_isInitializationComplete_m2F7E30B51DB12D34535BE7805A3CD490FFE12F68 (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRManagerSettings_InitializeLoaderSync_m5CE4139417252856F67537554BAD16798E5A8D6D (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* XRGeneralSettings_get_Instance_m9F222F982E62E066E119754858D8E73CFE42048C (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_x, Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___1_y, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* XRGeneralSettings_get_Manager_m112FEB4E6DFB7B5F5C4A2DEC4E975CF2EBD51B42 (XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Extensions_InitializeLoaderSyncIfNotInitialized_m838CDF88A619EF2AA04F49D0FF9A874CD57ADBD9 (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* ___0_settings, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* XRManagerSettings_get_activeLoader_mFB3B679005792D3DF871EAA7120DD86DCA1D5DEA_inline (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void _Manager__ctor_m11E7810A82B29A9721801336E8959ECB179B66A8 (_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRGeneralSettingsRemote__ctor_mA52C21119F543D37C4925C9EEE085464F686D0D0 (XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void _Loader__ctor_m6B54E50BC0A9C08EFDCC91981CF43A980CCFEDFD (_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRMeshSubsystemRemote__ctor_mDD727EE91F27DCA86A42F74511200E64F1889262 (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Extensions_InitializeLoaderSyncIfNotInitialized_m838CDF88A619EF2AA04F49D0FF9A874CD57ADBD9 (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* ___0_settings, const RuntimeMethod* method) 
{
	bool V_0 = false;
	{
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_0 = ___0_settings;
		NullCheck(L_0);
		bool L_1;
		L_1 = XRManagerSettings_get_isInitializationComplete_m2F7E30B51DB12D34535BE7805A3CD490FFE12F68(L_0, NULL);
		V_0 = (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
		bool L_2 = V_0;
		if (!L_2)
		{
			goto IL_0017;
		}
	}
	{
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_3 = ___0_settings;
		NullCheck(L_3);
		XRManagerSettings_InitializeLoaderSync_m5CE4139417252856F67537554BAD16798E5A8D6D(L_3, NULL);
	}

IL_0017:
	{
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRMeshSubsystemRemote_UnityEngine_ISubsystem_Start_mFD9D4950DDDBBFD646729299CF6791DE4395AB3B (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = ((XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_StaticFields*)il2cpp_codegen_static_fields_for(XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var))->___subsystemDelegate;
		NullCheck(L_0);
		InterfaceActionInvoker0::Invoke(0, ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var, L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRMeshSubsystemRemote_UnityEngine_ISubsystem_Stop_mABA4C38DB1AC542C0212C5552D4CFB80D59B6143 (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = ((XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_StaticFields*)il2cpp_codegen_static_fields_for(XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var))->___subsystemDelegate;
		NullCheck(L_0);
		InterfaceActionInvoker0::Invoke(1, ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var, L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRMeshSubsystemRemote_UnityEngine_ISubsystem_Destroy_m1D31792EC60644E0DEAF5FACDE2B4D3CCAE6CB52 (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = ((XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_StaticFields*)il2cpp_codegen_static_fields_for(XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var))->___subsystemDelegate;
		NullCheck(L_0);
		InterfaceActionInvoker0::Invoke(2, ISubsystem_t20F8E773FE1B915229571E9F78B3BC95C9E1CB67_il2cpp_TypeInfo_var, L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRMeshSubsystemRemote__ctor_mDD727EE91F27DCA86A42F74511200E64F1889262 (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2* XRGeneralSettingsRemote_GetRealSubsystem_mF6B39D067B28BA11E9C0C7968C8419CCF6C140CC (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRLoader_GetLoadedSubsystem_TisXRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2_m081FBFB83316621ED3D69C9D27376B59CB646519_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* V_0 = NULL;
	bool V_1 = false;
	XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* V_2 = NULL;
	bool V_3 = false;
	XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* V_4 = NULL;
	bool V_5 = false;
	XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2* V_6 = NULL;
	{
		il2cpp_codegen_runtime_class_init_inline(XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE_il2cpp_TypeInfo_var);
		XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* L_0;
		L_0 = XRGeneralSettings_get_Instance_m9F222F982E62E066E119754858D8E73CFE42048C(NULL);
		V_0 = L_0;
		XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* L_1 = V_0;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		V_1 = L_2;
		bool L_3 = V_1;
		if (!L_3)
		{
			goto IL_0051;
		}
	}
	{
		XRGeneralSettings_t8F8D096944606B5AD845D010706BF7094ADEC8CE* L_4 = V_0;
		NullCheck(L_4);
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_5;
		L_5 = XRGeneralSettings_get_Manager_m112FEB4E6DFB7B5F5C4A2DEC4E975CF2EBD51B42(L_4, NULL);
		V_2 = L_5;
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_6 = V_2;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_7;
		L_7 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_6, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		V_3 = L_7;
		bool L_8 = V_3;
		if (!L_8)
		{
			goto IL_0050;
		}
	}
	{
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_9 = V_2;
		Extensions_InitializeLoaderSyncIfNotInitialized_m838CDF88A619EF2AA04F49D0FF9A874CD57ADBD9(L_9, NULL);
		XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* L_10 = V_2;
		NullCheck(L_10);
		XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* L_11;
		L_11 = XRManagerSettings_get_activeLoader_mFB3B679005792D3DF871EAA7120DD86DCA1D5DEA_inline(L_10, NULL);
		V_4 = L_11;
		XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* L_12 = V_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_13;
		L_13 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_12, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		V_5 = L_13;
		bool L_14 = V_5;
		if (!L_14)
		{
			goto IL_004f;
		}
	}
	{
		XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* L_15 = V_4;
		NullCheck(L_15);
		XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2* L_16;
		L_16 = GenericVirtualFuncInvoker0< XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2* >::Invoke(XRLoader_GetLoadedSubsystem_TisXRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2_m081FBFB83316621ED3D69C9D27376B59CB646519_RuntimeMethod_var, L_15);
		V_6 = L_16;
		goto IL_0056;
	}

IL_004f:
	{
	}

IL_0050:
	{
	}

IL_0051:
	{
		V_6 = (XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2*)NULL;
		goto IL_0056;
	}

IL_0056:
	{
		XRMeshSubsystem_tDDC31EC10D4F0517542F9EB296428A0F7EC2C3B2* L_17 = V_6;
		return L_17;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRGeneralSettingsRemote__ctor_mA52C21119F543D37C4925C9EEE085464F686D0D0 (XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4* L_0 = (_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4*)il2cpp_codegen_object_new(_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4_il2cpp_TypeInfo_var);
		_Manager__ctor_m11E7810A82B29A9721801336E8959ECB179B66A8(L_0, NULL);
		__this->___Manager = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Manager), (void*)L_0);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void XRGeneralSettingsRemote__cctor_m6A6F0DD1D82A3E31ED4EBA4EC52D430F46025ED7 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF* L_0 = (XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF*)il2cpp_codegen_object_new(XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_il2cpp_TypeInfo_var);
		XRGeneralSettingsRemote__ctor_mA52C21119F543D37C4925C9EEE085464F686D0D0(L_0, NULL);
		((XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_StaticFields*)il2cpp_codegen_static_fields_for(XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_il2cpp_TypeInfo_var))->___Instance = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_StaticFields*)il2cpp_codegen_static_fields_for(XRGeneralSettingsRemote_t112BA539E206F4132EE80DBF24EE651205A375AF_il2cpp_TypeInfo_var))->___Instance), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void _Manager__ctor_m11E7810A82B29A9721801336E8959ECB179B66A8 (_Manager_t06B70FEA28FF9C08DCB861E17D91DAEFED704CC4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3* L_0 = (_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3*)il2cpp_codegen_object_new(_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var);
		_Loader__ctor_m6B54E50BC0A9C08EFDCC91981CF43A980CCFEDFD(L_0, NULL);
		__this->___activeLoader = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___activeLoader), (void*)L_0);
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void _Loader__ctor_m6B54E50BC0A9C08EFDCC91981CF43A980CCFEDFD (_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void _Loader__cctor_m730A96C2BBF97E53F538C97D3127ACCF32ED8392 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C* L_0 = (XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C*)il2cpp_codegen_object_new(XRMeshSubsystemRemote_tD69C7D28DA04E45F9BBA36B8252B1531D736DE7C_il2cpp_TypeInfo_var);
		XRMeshSubsystemRemote__ctor_mDD727EE91F27DCA86A42F74511200E64F1889262(L_0, NULL);
		((_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_StaticFields*)il2cpp_codegen_static_fields_for(_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var))->___subsystem = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_StaticFields*)il2cpp_codegen_static_fields_for(_Loader_t92CDEB8667583CFECD7536ED027E87113172E6B3_il2cpp_TypeInfo_var))->___subsystem), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* XRManagerSettings_get_activeLoader_mFB3B679005792D3DF871EAA7120DD86DCA1D5DEA_inline (XRManagerSettings_t7923B66EB3FEE58C7B9F85FF61749B774D3B9E52* __this, const RuntimeMethod* method) 
{
	{
		XRLoader_t80B1B1934C40561C5352ABC95D567DC2A7C9C976* L_0 = __this->___U3CactiveLoaderU3Ek__BackingField;
		return L_0;
	}
}
