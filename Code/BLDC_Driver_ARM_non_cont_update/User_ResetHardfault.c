/* Includes ------------------------------------------------------------------*/
#include "User_ResetHardfault.h"

/* Public variables ----------------------------------------------------------*/

/* Private types -------------------------------------------------------------*/
uint8_t Flag_ResetHardfault = 0;
/* Private const/macros ------------------------------------------------------*/
#define	RESET_DONE					1
#define	RESET_NOT						2
/* Private variables ---------------------------------------------------------*/

/* Private function prototypes -----------------------------------------------*/

/* Exported function body ----------------------------------------------------*/

/* Private functions body ----------------------------------------------------*/
void UWWDG_CheckResumed(void)
{
	if(RCC_GetFlagStatus(RCC_FLAG_WWDGRST) != RESET)
	{
		Flag_ResetHardfault = RESET_DONE;
		RCC_ClearFlag();
	}
	else
	{
		Flag_ResetHardfault = RESET_NOT;
	}
	
	if(SysTick_Config(SystemCoreClock / 1000))
	{
		while(1);
	}
}

void UWWDG_Configure(uint32_t WWDG_Prescaler, uint8_t WindowValue, uint8_t Counter)
{
	/* Enable WWDG clock */
  RCC_APB1PeriphClockCmd(RCC_APB1Periph_WWDG, ENABLE);

	/*   This parameter can be one of the following values:
  *     @arg WWDG_Prescaler_1: WWDG counter clock = (PCLK1/4096)/1
  *     @arg WWDG_Prescaler_2: WWDG counter clock = (PCLK1/4096)/2
  *     @arg WWDG_Prescaler_4: WWDG counter clock = (PCLK1/4096)/4
  *     @arg WWDG_Prescaler_8: WWDG counter clock = (PCLK1/4096)/8 */
/* On other devices, WWDG clock counter = (PCLK1(36MHz)/4096)/8 = 1099 Hz (~910 us)  */
  WWDG_SetPrescaler(WWDG_Prescaler);

  /* Set Window value to 80; WWDG counter should be refreshed only when the counter
    is below 80 (and greater than 64) otherwise a reset will be generated */
  WWDG_SetWindowValue(WindowValue);

  /* - On other devices
    Enable WWDG and set counter value to 127, WWDG timeout = ~910 us * 64 = 58.25 ms 
    In this case the refresh window is: ~910 us * (127-80) = 42.77 ms < refresh window < ~910 us * 64 = 58.25ms     
  */
  WWDG_Enable(Counter);
}

void UWWDG_UpdateWWDG(uint8_t Counter)
{
	/* Update WWDG counter */
  WWDG_SetCounter(Counter);
}

uint8_t UWWDG_GetFlagReset(void)
{
	return Flag_ResetHardfault;
}

void UWWDG_ResetSystem(void)
{
	/* As the following address is invalid (not mapped), a Hardfault exception
	  will be generated with an infinite loop and when the WWDG counter falls to 63
    the WWDG reset occurs */
	*(__IO uint32_t *) 0x000000FF = 0xFF;
}
/******************************************************************************/
/* Peripherals Interrupt Handlers --------------------------------------------*/
/******************************************************************************/
