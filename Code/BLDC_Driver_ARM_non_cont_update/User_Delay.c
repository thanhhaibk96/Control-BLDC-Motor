/* Includes ------------------------------------------------------------------*/
#include "User_Delay.h"

/* External functions define -------------------------------------------------*/

/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/


/* Private const/macros ------------------------------------------------------*/

/**
 * @defgroup Timer define
 * @{
 */
			/** 
			* @brief   Timer delay define
			*/

			#define 	UDELAY_TIM  				      		TIM3
			#define 	UDELAY_TIM_CLK					 	 		RCC_APB1Periph_TIM3	
			#define 	UDELAY_TIM_CLK_Cmd    				RCC_APB1PeriphClockCmd
/**
 * @}
 */
 

/* Private variables ---------------------------------------------------------*/
static TIM_TimeBaseInitTypeDef  TIM_TimeBaseStructure;
 
/* Private function prototypes -----------------------------------------------*/


/* Exported function body ----------------------------------------------------*/

void UDELAY_Configure(void)
{
	TIM_DeInit(UDELAY_TIM);
	UDELAY_TIM_CLK_Cmd(UDELAY_TIM_CLK, ENABLE);		

	TIM_TimeBaseStructure.TIM_Period = 65535;
  TIM_TimeBaseStructure.TIM_Prescaler = 0;
  TIM_TimeBaseStructure.TIM_ClockDivision = TIM_CKD_DIV1;
  TIM_TimeBaseStructure.TIM_CounterMode = TIM_CounterMode_Up;
		
	TIM_TimeBaseInit(UDELAY_TIM, &TIM_TimeBaseStructure);
}
 
void UDELAY_ms(uint32_t ms)
{	
		int i;
	
		TIM_TimeBaseStructure.TIM_Prescaler = 36-1;     // frequency = 1000000 (Hz) = 1 us
    TIM_TimeBaseStructure.TIM_Period = 1000 - 1;

		TIM_TimeBaseInit(UDELAY_TIM, &TIM_TimeBaseStructure);
	
		for (i = 0; i < ms; i++)
		{
			TIM_ClearFlag(UDELAY_TIM, TIM_FLAG_Update);
			
			TIM_Cmd(UDELAY_TIM, ENABLE);
					
			while (!TIM_GetFlagStatus(UDELAY_TIM, TIM_IT_Update));
						
			TIM_Cmd(UDELAY_TIM, DISABLE);
		}	
}


void UDELAY_us(uint32_t us)
{	
		int i;
	
		TIM_TimeBaseStructure.TIM_Prescaler = 18 - 1;     // frequency = 2000000 (Hz) = 0.5 us
    TIM_TimeBaseStructure.TIM_Period = 2 - 1;

		TIM_TimeBaseInit(UDELAY_TIM, &TIM_TimeBaseStructure);
	
		for (i = 0; i < us; i++)
		{
			TIM_ClearFlag(UDELAY_TIM, TIM_FLAG_Update);
			
			TIM_Cmd(UDELAY_TIM, ENABLE);
					
			while (!TIM_GetFlagStatus(UDELAY_TIM, TIM_IT_Update));
						
			TIM_Cmd(UDELAY_TIM, DISABLE);
		}	
}



/* Private functions body ----------------------------------------------------*/


