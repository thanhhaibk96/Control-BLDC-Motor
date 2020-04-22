/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_PWM_H
#define __USER_PWM_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"
#include "User_HALLSensor.h"
 /**
 * @defgroup <Group name here>
 * @{
 */	
 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
#define PWM_DC_MIN				81  //PWM duty cycle minimum >= 3 * Dead-time = 3 * 27(DutyCycle) = 81  (Dead-time = 7 duty cycle)
#define PWM_DC_MAX				3200	//PWM duty cycle maximum = Full_Duty - 3 * Dead-time = 3599 - 81 = 3566  For safety: 90% = 3209
	 
/* Exported function prototypes ----------------------------------------------*/
void UPWM_GPIO_Configure(void);
void UPWM_TIM_Configure(void);
void UPWM_ControlBLDCFET(void);
void UPWM_SetDutyCycle(uint32_t DutyCycle);
float UPWM_GetDutyCycle(void);
void UPWM_SetBytesDutyCycle(uint8_t a1, uint8_t a2, uint8_t a3, uint8_t a4);
void UPWM_StopPWM(void);
float UPWM_GetRefDutyCycle(void);
uint8_t UPWM_GetBytesDutyCycle(uint8_t byte);

	
/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
