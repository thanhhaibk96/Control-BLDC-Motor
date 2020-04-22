/**
  ******************************************************************************
  * @file    AUV_Driver_BLDC\BLDC_Driver_ARM_v3\main.c 
  * @author  Hai Chau Thanh
	* @mailbox thanhhaipif96@gmail.com
	* @company viamlab
  * @version ARM_V3.0.0
  * @date    01-March-2019
  * @brief   Main program body.
*/ 

/* Includes ------------------------------------------------------------------*/
#include "stm32f10x.h"
#include "User_PWM.h"
#include "User_HALLSensor.h"
#include "User_CAN.h"
#include "User_ADC.h"
#include "User_ResetHardfault.h"
#include "User_Algorithm.h"
#include "User_Delay.h"

/* Private typedef -----------------------------------------------------------*/
RCC_ClocksTypeDef 				RCC_ClockFreq;
ErrorStatus 							HSEStartUpStatus;

void Clock_HSE_Configuration(uint32_t clock_in);
/* Private define ------------------------------------------------------------*/
#define clock_16Mhz  0
#define clock_24Mhz  0x00040000
#define clock_32Mhz  0x00080000
#define clock_40Mhz  0x000C0000
#define clock_48Mhz  0x00100000
#define clock_56Mhz  0x00140000
#define clock_64Mhz  0x00180000
#define clock_72Mhz  0x001C0000

/* Private macro -------------------------------------------------------------*/
/* Private variables ---------------------------------------------------------*/
__IO uint32_t TimingDelay = 0;
float A,B,C;
float PWM;
uint16_t pHall;
uint8_t CAN_Data[8] = {'C',0x02,0x03,0x04,0x05,0x06,0x07,0x08};
uint32_t clock_source = 0;
uint8_t CAN_count = 0;
/* Private function prototypes -----------------------------------------------*/
/* Private functions ---------------------------------------------------------*/
void Delay(__IO uint32_t nTime);
int main(void)
{ 
	Clock_HSE_Configuration(clock_72Mhz);
	RCC_GetClocksFreq(&RCC_ClockFreq);			
	clock_source = RCC_ClockFreq.SYSCLK_Frequency; 
	
	// Update SystemCoreClock value
	SystemCoreClockUpdate();
	// Configure the SysTick timer to overflow every 1 us
//	SysTick_Config(SystemCoreClock / 1000000);
	UDELAY_Configure();
	
	UPWM_GPIO_Configure();
	UPWM_TIM_Configure();
	UHALL_GPIO_Configure();
	UCAN_GPIO_Configure();
	UCAN_CAN_Configure();
	UHALL_TIM_Configure();
	UPWM_StopPWM();
	UADC_GPIO_Configure();
	UADC_ADC_DMA_Configure();
	
	UDELAY_ms(2000);
	//---------------------------------Initial----------------------------------//
	UALTHM_UpdateParameters(Update_Kp,0,0,0,0);
	UALTHM_UpdateParameters(Update_Ki,0,0,0,0);
	UALTHM_UpdateParameters(Update_Kd,0,0,0,0);
	UALTHM_UpdateParameters(Update_Ge,0,0,0,0);
	UALTHM_UpdateParameters(Update_Gde,0,0,0,0);
	UALTHM_UpdateParameters(Update_Gdu,0,0,0,0);
	UALTHM_UpdateParameters(Update_SetSpeed,0,0,0,0);
  while (1)
  {
//		pHall = UHALL_GetPosition();
//		CAN_Data[1] = CAN_count;
//		UCAN_Transmit(CAN1,0x123,8,CAN_Data);
//		CAN_count++;
//		UHALL_ReadPosition(Motor_CW);
//		UPWM_SetBytesDutyCycle(0x00, 0x00, 0xA0, 0x41);
//		PWM = UHALL_GetActualSpeed();
//		A = UADC_GetValue(Temp_On_Chip);
//		B = UADC_GetValue(Temp_LM35);
//		C = UADC_GetValue(iMotor);
//		Delay(300);
	}
}

/**
  * @brief  Inserts a delay time.
  * @param  nTime: specifies the delay time length, in milliseconds.
  * @retval None
  */
void Delay(__IO uint32_t nTime)
{ 
  TimingDelay = nTime;
  while(TimingDelay != 0)
  {
  }
}

void Clock_HSE_Configuration(uint32_t clock_in)
{
  RCC_DeInit(); 														
  /* Enable HSE */
  RCC_HSEConfig(RCC_HSE_ON);									
  /* Wait till HSE is ready */
  HSEStartUpStatus = RCC_WaitForHSEStartUp();
  if (HSEStartUpStatus == SUCCESS)
  {
    /* Enable Prefetch Buffer */
    FLASH_PrefetchBufferCmd(FLASH_PrefetchBuffer_Enable);
    /* Flash 2 wait state */
    FLASH_SetLatency(FLASH_Latency_2);
    /* HCLK = SYSCLK */
    RCC_HCLKConfig(RCC_SYSCLK_Div1); 
    /* PCLK2 = HCLK */
    RCC_PCLK2Config(RCC_HCLK_Div1); 
    /* PCLK1 = HCLK/2 */
    RCC_PCLK1Config(RCC_HCLK_Div2);
    /* PLLCLK = clock_in */
			RCC_PLLConfig(RCC_PLLSource_HSE_Div1, clock_in);	
    /* Enable PLL */ 
    RCC_PLLCmd(ENABLE);
    /* Wait till PLL is ready */
    while (RCC_GetFlagStatus(RCC_FLAG_PLLRDY) == RESET) {}
    /* Select PLL as system clock source */
    RCC_SYSCLKConfig(RCC_SYSCLKSource_PLLCLK);
    /* Wait till PLL is used as system clock source */
    while(RCC_GetSYSCLKSource() != 0x08){ }
  }
  else{ while (1){}}
}

#ifdef  USE_FULL_ASSERT

/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t* file, uint32_t line)
{ 
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */

  /* Infinite loop */
  while (1)
  {
  }
}

#endif

/**
  * @}
  */

/**
  * @}
  */

/******************* (C) COPYRIGHT 2011 STMicroelectronics *****END OF FILE****/
