/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_RESETHARDFAULT_H
#define __USER_RESETHARDFAULT_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 


	 
/* Exported constants/macros -------------------------------------------------*/


 /**
 * @defgroup <Group name here>
 * @{
 */	
 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
	


	 
/* Exported function prototypes ----------------------------------------------*/
/* Check if the system has resumed from WWDG reset */
void UWWDG_CheckResumed(void);
void UWWDG_Configure(uint32_t WWDG_Prescaler, uint8_t WindowValue, uint8_t Counter);
uint8_t UWWDG_GetFlagReset(void);
void UWWDG_UpdateWWDG(uint8_t Counter);
void UWWDG_ResetSystem(void);
/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
