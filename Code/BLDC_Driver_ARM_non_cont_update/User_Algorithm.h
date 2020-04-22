/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __USER_ALGORITHM_H
#define __USER_ALGORITHM_H

#ifdef __cplusplus
 extern "C" {
#endif
	
	 
/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"	 
#include "User_CAN.h"
#include <stdbool.h>
	 
/* Exported constants/macros -------------------------------------------------*/
#define UALTHM_TIME_SAMPLING						10 //ms
	 
 /**
 * @defgroup <Group name here>
 * @{
 */	
 
 /**
 * @}
 */	 
/* Exported types ------------------------------------------------------------*/
#define Update_Kp					0
#define	Update_Ki					1
#define Update_Kd					2
#define	Update_Ge					3
#define Update_Gde				4
#define	Update_Gdu				5
#define Update_SetSpeed 	6

	 
/* Exported function prototypes ----------------------------------------------*/
void UALTHM_PID_v1(bool _state_Motor);
void UALTHM_PID_v2(bool _state_Motor);
void UALTHM_FUZZY(bool _state_Motor);
int UFUZZY_Trap_mf(int x,int a,int b,int c,int d);
void UALTHM_UpdateParameters(uint8_t type, uint8_t a1, uint8_t a2, uint8_t a3, uint8_t a4);
uint8_t UALTHM_GetBytesSetSpeed(uint8_t byte);
void UALTHM_Controller(bool _state_Motor);
/* Peripherals Interrupt prototypes ------------------------------------------*/


#ifdef __cplusplus
}
#endif

#endif 
