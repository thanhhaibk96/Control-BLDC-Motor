/* Includes ------------------------------------------------------------------*/
#include "User_PWM.h"
#include "User_Algorithm.h"
#include "User_CAN.h"

/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/
uint16_t UPWM_TimerPeriod = 0;

static union CAN_UpdateData
{
	float Value;
	
	struct
	{
		unsigned a1:8;
		unsigned a2:8;
		unsigned a3:8;
		unsigned a4:8;
	} byte;
} UPWM_DutyCycle, UPWM_RefDutyCycle;

/* Private const/macros ------------------------------------------------------*/

/* Private variables ---------------------------------------------------------*/

/* Private function prototypes -----------------------------------------------*/

/* Exported function body ----------------------------------------------------*/

/* Private functions body ----------------------------------------------------*/
	void UPWM_GPIO_Configure(void)
	{
		GPIO_InitTypeDef GPIO_InitStruct;
		
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOA,ENABLE);
		GPIO_InitStruct.GPIO_Mode = GPIO_Mode_AF_PP;
		GPIO_InitStruct.GPIO_Pin = GPIO_Pin_8|GPIO_Pin_9|GPIO_Pin_10;
		GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
		GPIO_Init(GPIOA,&GPIO_InitStruct);
		
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOB,ENABLE);
		GPIO_InitStruct.GPIO_Mode = GPIO_Mode_AF_PP;
		GPIO_InitStruct.GPIO_Pin = GPIO_Pin_13|GPIO_Pin_14|GPIO_Pin_15;
		GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
		GPIO_Init(GPIOB,&GPIO_InitStruct);
		
		GPIO_InitStruct.GPIO_Mode = GPIO_Mode_IN_FLOATING;
		GPIO_InitStruct.GPIO_Pin = GPIO_Pin_12;
		GPIO_InitStruct.GPIO_Speed = GPIO_Speed_50MHz;
		GPIO_Init(GPIOB,&GPIO_InitStruct);
		
	}

	void UPWM_TIM_Configure(void)
	{
		TIM_TimeBaseInitTypeDef TIM_TimeBaseInitStruct;
		TIM_OCInitTypeDef TIM_OCInitStruct;
		TIM_BDTRInitTypeDef TIM_BDTRInitStruct;
		
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_TIM1|RCC_APB2Periph_AFIO,ENABLE);
		
		UPWM_TimerPeriod = (SystemCoreClock / 20000) - 1;
		
		UPWM_DutyCycle.Value = 0;
		/* Time Base configuration */
		TIM_TimeBaseInitStruct.TIM_Prescaler = 1 - 1;
		TIM_TimeBaseInitStruct.TIM_CounterMode = TIM_CounterMode_Up;
		TIM_TimeBaseInitStruct.TIM_Period = UPWM_TimerPeriod;
		TIM_TimeBaseInitStruct.TIM_ClockDivision = TIM_CKD_DIV1;
		TIM_TimeBaseInitStruct.TIM_RepetitionCounter = 0;

		TIM_TimeBaseInit(TIM1, &TIM_TimeBaseInitStruct);

		TIM_OCInitStruct.TIM_OCMode = TIM_OCMode_Timing;
		TIM_OCInitStruct.TIM_OutputState = TIM_OutputState_Enable;
		TIM_OCInitStruct.TIM_OutputNState = TIM_OutputNState_Enable;
		TIM_OCInitStruct.TIM_Pulse = (uint16_t)UPWM_DutyCycle.Value;
		TIM_OCInitStruct.TIM_OCPolarity = TIM_OCPolarity_High;
		TIM_OCInitStruct.TIM_OCNPolarity = TIM_OCNPolarity_High;
		TIM_OCInitStruct.TIM_OCIdleState = TIM_OCIdleState_Reset;
		TIM_OCInitStruct.TIM_OCNIdleState = TIM_OCIdleState_Reset;

		TIM_OC1Init(TIM1, &TIM_OCInitStruct);
		TIM_OC2Init(TIM1, &TIM_OCInitStruct);
		TIM_OC3Init(TIM1, &TIM_OCInitStruct);
		
		TIM_OC1PreloadConfig(TIM1, TIM_OCPreload_Enable);
		TIM_OC2PreloadConfig(TIM1, TIM_OCPreload_Enable);
		TIM_OC3PreloadConfig(TIM1, TIM_OCPreload_Enable);
		
		TIM_BDTRInitStruct.TIM_OSSRState = TIM_OSSRState_Enable;
		TIM_BDTRInitStruct.TIM_OSSIState = TIM_OSSIState_Enable;
		TIM_BDTRInitStruct.TIM_LOCKLevel = TIM_LOCKLevel_OFF;
		TIM_BDTRInitStruct.TIM_DeadTime = 150;
		TIM_BDTRInitStruct.TIM_Break = TIM_Break_Disable;
		TIM_BDTRInitStruct.TIM_BreakPolarity = TIM_BreakPolarity_High;
		TIM_BDTRInitStruct.TIM_AutomaticOutput = TIM_AutomaticOutput_Enable;

		TIM_BDTRConfig(TIM1, &TIM_BDTRInitStruct);
		
		/* TIM1 counter enable */
		TIM_Cmd(TIM1, ENABLE);

		/* Main Output Enable */
		TIM_CtrlPWMOutputs(TIM1, ENABLE);
	}
	
	void UPWM_ControlBLDCFET(void)
	{
		// Phase A
		if(UHALL_GetHALLHA())
		{
			TIM_SelectOCxM(TIM1,TIM_Channel_1,TIM_OCMode_PWM1);
			TIM_CCxCmd(TIM1,TIM_Channel_1,TIM_CCx_Enable);
			TIM_CCxNCmd(TIM1,TIM_Channel_1,TIM_CCxN_Enable);
		}
		else
		{
			TIM_CCxCmd(TIM1,TIM_Channel_1,TIM_CCx_Disable);
			
			if(UHALL_GetHALLLA())
			{
				TIM_SelectOCxM(TIM1,TIM_Channel_1,TIM_ForcedAction_Active);
				TIM_CCxNCmd(TIM1,TIM_Channel_1,TIM_CCxN_Enable);
			}
			else
			{
				TIM_CCxNCmd(TIM1,TIM_Channel_1,TIM_CCxN_Disable);
			}
		}
		
		// Phase B
		if(UHALL_GetHALLHB())
		{
			TIM_SelectOCxM(TIM1,TIM_Channel_2,TIM_OCMode_PWM1);
			TIM_CCxCmd(TIM1,TIM_Channel_2,TIM_CCx_Enable);
			TIM_CCxNCmd(TIM1,TIM_Channel_2,TIM_CCxN_Enable);
		}
		else
		{
			TIM_CCxCmd(TIM1,TIM_Channel_2,TIM_CCx_Disable);
			
			if(UHALL_GetHALLLB())
			{
				TIM_SelectOCxM(TIM1,TIM_Channel_2,TIM_ForcedAction_Active);
				TIM_CCxNCmd(TIM1,TIM_Channel_2,TIM_CCxN_Enable);
			}
			else
			{
				TIM_CCxNCmd(TIM1,TIM_Channel_2,TIM_CCxN_Disable);
			}
		}
		
		// Phase C
		if(UHALL_GetHALLHC())
		{
			TIM_SelectOCxM(TIM1,TIM_Channel_3,TIM_OCMode_PWM1);
			TIM_CCxCmd(TIM1,TIM_Channel_3,TIM_CCx_Enable);
			TIM_CCxNCmd(TIM1,TIM_Channel_3,TIM_CCxN_Enable);
		}
		else
		{
			TIM_CCxCmd(TIM1,TIM_Channel_3,TIM_CCx_Disable);
			
			if(UHALL_GetHALLLC())
			{
				TIM_SelectOCxM(TIM1,TIM_Channel_3,TIM_ForcedAction_Active);
				TIM_CCxNCmd(TIM1,TIM_Channel_3,TIM_CCxN_Enable);
			}
			else
			{
				TIM_CCxNCmd(TIM1,TIM_Channel_3,TIM_CCxN_Disable);
			}
		}
	}

void UPWM_SetDutyCycle(uint32_t DutyCycle)
{	
	uint16_t Duty = (uint16_t)((float)DutyCycle*(float)PWM_DC_MAX/100.0);
	if((Duty >= PWM_DC_MIN)&&(Duty <= PWM_DC_MAX))
	{
		TIM_SetCompare1(TIM1, Duty);
		TIM_SetCompare2(TIM1, Duty);
		TIM_SetCompare3(TIM1, Duty);
		UHALL_MotorStatus(RUN);
	}
 
	else if(Duty > PWM_DC_MAX)
	{
		TIM_SetCompare1(TIM1, PWM_DC_MAX);
		TIM_SetCompare2(TIM1, PWM_DC_MAX);
		TIM_SetCompare3(TIM1, PWM_DC_MAX);
		UHALL_MotorStatus(RUN);
	}
	else if(Duty < PWM_DC_MIN)
	{
		UHALL_MotorStatus(STOP);
		TIM_SetCompare1(TIM1, 0);
		TIM_SetCompare2(TIM1, 0);
		TIM_SetCompare3(TIM1, 0);
		UPWM_StopPWM();		
	}
}
//------------------------------DutyCycle Get From PC-----------------------------/
void UPWM_SetBytesDutyCycle(uint8_t a1, uint8_t a2, uint8_t a3, uint8_t a4)
{
	UPWM_DutyCycle.byte.a1 = a1;
	UPWM_DutyCycle.byte.a2 = a2;
	UPWM_DutyCycle.byte.a3 = a3;
	UPWM_DutyCycle.byte.a4 = a4;
	
	UPWM_SetDutyCycle(UPWM_DutyCycle.Value);
}
//------------------------------DutyCycle Send To PC-----------------------------/
uint8_t UPWM_GetBytesDutyCycle(uint8_t byte)
{
	UPWM_RefDutyCycle.Value = (float)((float)TIM_GetCapture1(TIM1)*100.0/(float)PWM_DC_MAX);
	uint8_t value = 0;
	if(byte == 1) value = UPWM_RefDutyCycle.byte.a1;
	else if( byte == 2) value = UPWM_RefDutyCycle.byte.a2;
	else if( byte == 3) value = UPWM_RefDutyCycle.byte.a3;
	else if (byte == 4) value = UPWM_RefDutyCycle.byte.a4;
	return value;
}

float UPWM_GetRefDutyCycle(void)
{
	UPWM_RefDutyCycle.Value = (float)((float)TIM_GetCapture1(TIM1)*100.0/(float)PWM_DC_MAX);
	return UPWM_RefDutyCycle.Value;
}

float UPWM_GetDutyCycle(void)
{
	return UPWM_DutyCycle.Value;
}

void UPWM_StopPWM(void)
{
	TIM_Cmd(TIM4,DISABLE);
	TIM_SetCounter(TIM4,0);
	UCAN_ResetFlag();
	TIM_CCxCmd(TIM1,TIM_Channel_1,TIM_CCx_Disable);
	TIM_CCxNCmd(TIM1,TIM_Channel_1,TIM_CCxN_Disable);
	TIM_CCxCmd(TIM1,TIM_Channel_2,TIM_CCx_Disable);
	TIM_CCxNCmd(TIM1,TIM_Channel_2,TIM_CCxN_Disable);
	TIM_CCxCmd(TIM1,TIM_Channel_3,TIM_CCx_Disable);
	TIM_CCxNCmd(TIM1,TIM_Channel_3,TIM_CCxN_Disable);
	UHALL_ResetPosHall();
}
