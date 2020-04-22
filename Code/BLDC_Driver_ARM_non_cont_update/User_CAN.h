/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_CAN_H
#define __USER_CAN_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 
#include <stdbool.h>
#include "User_Delay.h"
	 
/* Exported constants/macros -------------------------------------------------*/


 /**
 * @defgroup <Group name here>
 * @{
 */	
 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
#define Flag_Open_Loop					0
#define Flag_PID_Control				1
#define Flag_Fuzzy_Control			2
#define Flag_Motor_Run_CW				3
#define Flag_Motor_Run_CCW			4
	 
/* Exported function prototypes ----------------------------------------------*/
void UCAN_GPIO_Configure(void);
void UCAN_CAN_Configure(void);
void UCAN_Transmit(CAN_TypeDef* CANx, int _IDstd,int _length, uint8_t _data[]);
uint8_t UCAN_GetMessage(uint8_t bytes);
bool UCAN_GetFlag(uint8_t type);
void UCAN_ResetFlag(void);
uint8_t UCAN_Checksum(uint8_t *_data);
void UCAN_SystemOverLoad(void);
void UCAN_StickedMotor(void);

/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
