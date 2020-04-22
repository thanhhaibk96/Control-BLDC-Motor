/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_HALLSENSOR_H
#define __USER_HALLSENSOR_H

#ifdef __cplusplus
 extern "C" {
#endif
	

/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 
#include <stdint.h>
#include <stdbool.h>
/* Exported constants/macros -------------------------------------------------*/
#define Motor_CW		0
#define Motor_CCW		1
typedef enum {STOP = 0, RUN = !STOP} StatusofMotor;
 /**
 * @defgroup <Group name here>
 * @{
 */	
void UHALL_MotorStatus(StatusofMotor _StatusMotor);
	 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
	


	 
/* Exported function prototypes ----------------------------------------------*/
void UHALL_GPIO_Configure(void);
void UHALL_TIM_Configure(void);
void UHALL_ReadPosition(bool _state_Motor);
uint8_t UHALL_GetHALLHA(void);
uint8_t UHALL_GetHALLLA(void);
uint8_t UHALL_GetHALLHB(void);
uint8_t UHALL_GetHALLLB(void);
uint8_t UHALL_GetHALLHC(void);
uint8_t UHALL_GetHALLLC(void);
uint16_t UHALL_GetPosition(void);
float UHALL_GetActualSpeed(void);
uint8_t UHALL_GetBytesActualSpeed(uint8_t byte);
void UHALL_ResetPosHall(void);
/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
