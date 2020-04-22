/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_ADC_H
#define __USER_ADC_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 


	 
/* Exported constants/macros -------------------------------------------------*/
typedef enum 
{
	Temp_On_Chip = 0,
	iMotor = 2,
	Vref = 3,
	Temp_LM35 = 1,
}UADC_Channel_TypeDef;


 /**
 * @defgroup <Group name here>
 * @{
 */	
 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
	


	 
/* Exported function prototypes ----------------------------------------------*/
void UADC_GPIO_Configure(void);
void UADC_ADC_DMA_Configure(void);
uint8_t UADC_GetBytesConverted(UADC_Channel_TypeDef _channel, uint8_t byte);
uint16_t UADC_GetAvrValue(UADC_Channel_TypeDef _channel);
float UADC_GetValue(UADC_Channel_TypeDef _channel);
/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
