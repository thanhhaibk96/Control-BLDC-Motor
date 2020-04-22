/* Includes ------------------------------------------------------------------*/
#include "User_Algorithm.h"
#include "User_HALLSensor.h"
#include "User_PWM.h"
#include "User_CAN.h"

/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/
static union Data_Algorithm
{
	float Value;
	
	struct
	{
		unsigned a1:8;
		unsigned a2:8;
		unsigned a3:8;
		unsigned a4:8;
	} byte;
} UpdateParameters;

static struct
{
	float Kp;
	float Ki;
	float Kd;
}	PID_Parameter;

static struct
{
	float Ge;
	float Gde;
	float Gdu;
} Fuzzy_Parameter;

static struct 
{
	float P;
	float I;
	float D;
}	PID_part;

static float UALTHM_SetSpeed = 0, SpeedError = 0, Pre_SpeedError = 0, Pre_Pre_SpeedError = 0;
static int _DutyCycle = 0;
/* Private const/macros ------------------------------------------------------*/
#define DU_NB 	-3
#define DU_NM 	-2
#define DU_NS 	-1
#define DU_ZE 	 0
#define DU_PS 	 1
#define DU_PM 	 2
#define DU_PB 	 3
#define min(x,y) ((x<y)?(x):(y))
/* Private variables ---------------------------------------------------------*/

/* Private function prototypes -----------------------------------------------*/

/* Exported function body ----------------------------------------------------*/

/* Private functions body ----------------------------------------------------*/
void UALTHM_UpdateParameters(uint8_t type, uint8_t a1, uint8_t a2, uint8_t a3, uint8_t a4)
{
	UpdateParameters.byte.a1 = a1;
	UpdateParameters.byte.a2 = a2;
	UpdateParameters.byte.a3 = a3;
	UpdateParameters.byte.a4 = a4;
	
	switch(type)
	{
		case Update_Kp:
		{
			PID_Parameter.Kp = UpdateParameters.Value;
			break;
		}
		case Update_Ki:
		{
			PID_Parameter.Ki = UpdateParameters.Value;
			break;
		}
		case Update_Kd:
		{
			PID_Parameter.Kd = UpdateParameters.Value;
			break;
		}
		case Update_Ge:
		{
			Fuzzy_Parameter.Ge = UpdateParameters.Value;
			break;
		}
		case Update_Gde:
		{
			Fuzzy_Parameter.Gde = UpdateParameters.Value;
			break;
		}
		case Update_Gdu:
		{
			Fuzzy_Parameter.Gdu = UpdateParameters.Value;
			break;
		}
		case Update_SetSpeed:
		{
			UALTHM_SetSpeed = (int)UpdateParameters.Value;
		}
	}
}

uint8_t UALTHM_GetBytesSetSpeed(uint8_t byte)
{
	UpdateParameters.Value = (float)UALTHM_SetSpeed;
	uint8_t value = 0;
	if(byte == 1) value = UpdateParameters.byte.a1;
	else if( byte == 2) value = UpdateParameters.byte.a2;
	else if( byte == 3) value = UpdateParameters.byte.a3;
	else if (byte == 4) value = UpdateParameters.byte.a4;
	return value;
}

void UALTHM_PID_v1(bool _state_Motor)
{
	SpeedError = UALTHM_SetSpeed - UHALL_GetActualSpeed();
	PID_part.P = PID_Parameter.Kp*SpeedError;
	PID_part.I += PID_Parameter.Ki*SpeedError*((float)UALTHM_TIME_SAMPLING/1000.0);
	PID_part.D = PID_Parameter.Kd*(SpeedError - Pre_SpeedError)/((float)UALTHM_TIME_SAMPLING/1000.0);
	
	if(PID_part.I < -(float)PWM_DC_MAX/10.0) PID_part.I = -(float)PWM_DC_MAX/10.0;
	else if(PID_part.I > (float)PWM_DC_MAX/10.0) PID_part.I = (float)PWM_DC_MAX/10.0;
	
	_DutyCycle += (int)(PID_part.P + PID_part.I + PID_part.D);
	Pre_SpeedError = SpeedError;
	
	if(_DutyCycle < PWM_DC_MIN)
	{
//		_DutyCycle = 0;
		PID_part.I = 0;
	}
	else if(_DutyCycle > PWM_DC_MAX)
	{
//		_DutyCycle = PWM_DC_MAX;
		PID_part.I = 0;
	}
	
	UHALL_ReadPosition(_state_Motor);
	UPWM_SetDutyCycle(_DutyCycle);
}

void UALTHM_PID_v2(bool _state_Motor)
{	
	float Time_Sampling = (float)UALTHM_TIME_SAMPLING/1000.0;
	SpeedError = UALTHM_SetSpeed - UHALL_GetActualSpeed();
	
	PID_part.P = 2*Time_Sampling*PID_Parameter.Kp + PID_Parameter.Ki*Time_Sampling*Time_Sampling + 2*PID_Parameter.Kd;
	PID_part.I = PID_Parameter.Ki*Time_Sampling*Time_Sampling - 2*Time_Sampling*PID_Parameter.Kp - 4*PID_Parameter.Kd;
	PID_part.D = 2*PID_Parameter.Kd;
	
	_DutyCycle += (int)((PID_part.P*SpeedError + PID_part.I*Pre_SpeedError + PID_part.D*Pre_Pre_SpeedError)/(2*Time_Sampling));
	
	Pre_Pre_SpeedError = Pre_SpeedError;
	Pre_SpeedError = SpeedError;
	
	if(_DutyCycle < PWM_DC_MIN)
	{
//		_DutyCycle = 0;
		PID_part.I = 0;
	}
	else if(_DutyCycle > PWM_DC_MAX)
	{
//		_DutyCycle = PWM_DC_MAX;
		PID_part.I = 0;
	}
		
	UHALL_ReadPosition(_state_Motor);
	UPWM_SetDutyCycle(_DutyCycle);
}

void UALTHM_FUZZY(bool _state_Motor)
{
	  int ChangeSpeedError;
    int i;
    int x1,x2;
    int x1_NB,x1_NS,x1_ZE,x1_PS,x1_PB;
    int x2_NE,x2_ZE,x2_PO;
    
    int fuzzy_value[15];
    int du_value[15] = {DU_NB,DU_NM,DU_NS,
                        DU_NM,DU_NS,DU_ZE,
                        DU_NS,DU_ZE,DU_PS,
                        DU_ZE,DU_PS,DU_PM,
                        DU_PS,DU_PM,DU_PB};
    
    int du_fuzzy = 0;
    
    SpeedError = UALTHM_SetSpeed - UHALL_GetActualSpeed();
    ChangeSpeedError = SpeedError - Pre_SpeedError;
    Pre_SpeedError = SpeedError;
    
    x1 = SpeedError * 100 / Fuzzy_Parameter.Ge;
    x2 = ChangeSpeedError * 100 / UALTHM_TIME_SAMPLING / Fuzzy_Parameter.Gde;
    
    if (x1>100)            x1 = 100;
    else if (x1<-100)      x1 = -100;
    
    if (x2>100)            x2 = 100;
    else if (x2<-100)      x2 = -100;
    
    x1_NB = UFUZZY_Trap_mf(x1,-200,-100,-50,-20);
    x1_NS = UFUZZY_Trap_mf(x1,-50,-20,-20,0);
    x1_ZE = UFUZZY_Trap_mf(x1,-20,0,0,20);
    x1_PS = UFUZZY_Trap_mf(x1,0,20,20,50);
    x1_PB = UFUZZY_Trap_mf(x1,20,50,100,200);
    
    x2_NE = UFUZZY_Trap_mf(x2,-200,-100,-30,0);
    x2_ZE = UFUZZY_Trap_mf(x2,-30,0,0,30);
    x2_PO = UFUZZY_Trap_mf(x2,0,30,100,200);
    
    /*
    Percentage_DC.value = (float)x2;
    Des_Speed.value = (float)x2_NE;
    r_Speed.value = (float)(x2_ZE);
    Current.value = (float)(x2_PO);
    */
    
    fuzzy_value[0] = min(x1_NB,x2_NE);
    fuzzy_value[1] = min(x1_NB,x2_ZE);
    fuzzy_value[2] = min(x1_NB,x2_PO);
    fuzzy_value[3] = min(x1_NS,x2_NE);
    fuzzy_value[4] = min(x1_NS,x2_ZE);
    fuzzy_value[5] = min(x1_NS,x2_PO);
    fuzzy_value[6] = min(x1_ZE,x2_NE);
    fuzzy_value[7] = min(x1_ZE,x2_ZE);
    fuzzy_value[8] = min(x1_ZE,x2_PO);
    fuzzy_value[9] = min(x1_PS,x2_NE);
    fuzzy_value[10] = min(x1_PS,x2_ZE);
    fuzzy_value[11] = min(x1_PS,x2_PO);
    fuzzy_value[12] = min(x1_PB,x2_NE);
    fuzzy_value[13] = min(x1_PB,x2_ZE);
    fuzzy_value[14] = min(x1_PB,x2_PO);
    
    for (i=0;i<15;i++) du_fuzzy += du_value[i] * fuzzy_value[i];
    
    _DutyCycle += (int)( ( ( (long int)(du_fuzzy) ) * UALTHM_TIME_SAMPLING ) * Fuzzy_Parameter.Gdu ); 
		
		if(_DutyCycle <= 0)
		{
			_DutyCycle = 0;
//			UPWM_StopPWM();
//			UPWM_SetDutyCycle(_DutyCycle);
		}
		else
		{
//			if(_DutyCycle < PWM_DC_MIN) _DutyCycle = 0;
//			else if(_DutyCycle > PWM_DC_MAX) _DutyCycle = PWM_DC_MAX;
			
			UHALL_ReadPosition(_state_Motor);
			UPWM_SetDutyCycle(_DutyCycle);			
		}
}

int UFUZZY_Trap_mf(int x,int a,int b,int c,int d)
{
	int value;
  if (x<a) value = 0;
  else if (x<b) value = 100*(x-a)/(b-a);
  else if (x<c) value = 100;
  else if (x<d) value = 100*(d-x)/(d-c);
  else value = 0;
  return value;
}

void UALTHM_Controller(bool _state_Motor)
{
	if(UCAN_GetFlag(Flag_Open_Loop) == 0)
	{
		if(UCAN_GetFlag(Flag_PID_Control))
		{
			UALTHM_PID_v1(_state_Motor);
		}
		else if(UCAN_GetFlag(Flag_Fuzzy_Control))
		{
			UALTHM_FUZZY(_state_Motor);
		}
		if((UALTHM_SetSpeed == 0)&&(UPWM_GetRefDutyCycle() < 5.0))
		{
			UPWM_StopPWM();
			UPWM_SetDutyCycle(0);
		}
	}
}

/******************************************************************************/
/* Peripherals Interrupt Handlers --------------------------------------------*/
/******************************************************************************/
