/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_DELAY_H
#define __USER_DELAY_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 
#include "stm32f10x_tim.h"
	 
/* Exported constants/macros -------------------------------------------------*/


/* Exported types ------------------------------------------------------------*/
	 

	 
/* Exported function prototypes ----------------------------------------------*/
void UDELAY_Configure(void);	 

void UDELAY_ms(uint32_t ms);
void UDELAY_us(uint32_t us);
	 
#ifdef __cplusplus
}
#endif

#endif 
