// Copyright Epic Games, Inc. All Rights Reserved.

#include "AlgorandBPLibrary.h"
#include "Algorand.h"
#include "UnrealCLRLibrary.h"
#include "GenericPlatform/GenericPlatformMisc.h"

UAlgorandBPLibrary::UAlgorandBPLibrary(const FObjectInitializer& ObjectInitializer)
: Super(ObjectInitializer)
{

}

void UAlgorandBPLibrary::CreateWallet(AActor* accountActor)
{
	bool found;
	FManagedFunction CLR_CreateWallet = UUnrealCLRLibrary::FindManagedFunction("Game.System.CreateAccount", false, found);
	UUnrealCLRLibrary::ExecuteManagedFunction(CLR_CreateWallet, accountActor);
}

void UAlgorandBPLibrary::LoadAccount(AActor* accountActor)
{
	bool found;
	FManagedFunction CLR_LoadAccount = UUnrealCLRLibrary::FindManagedFunction("Game.System.LoadAccount", false, found);
	UUnrealCLRLibrary::ExecuteManagedFunction(CLR_LoadAccount, accountActor);
}

void UAlgorandBPLibrary::UpdateAccount(AActor* accountActor)
{
	bool found;
	FManagedFunction CLR_UpdateAccount = UUnrealCLRLibrary::FindManagedFunction("Game.System.UpdateAccount", false, found);
	UUnrealCLRLibrary::ExecuteManagedFunction(CLR_UpdateAccount, accountActor);
}

void UAlgorandBPLibrary::SendTransaction(AActor* transactionActor)
{
	bool found;
	FManagedFunction CLR_SendTransaction = UUnrealCLRLibrary::FindManagedFunction("Game.System.SendTransaction", false, found);
	UUnrealCLRLibrary::ExecuteManagedFunction(CLR_SendTransaction, transactionActor);
}

void UAlgorandBPLibrary::CopyToClipboard(FString Source)
{
	FGenericPlatformMisc::ClipboardCopy(*Source);
}

void UAlgorandBPLibrary::PasteFromClipboard(FString& Destination)
{
	FGenericPlatformMisc::ClipboardPaste(Destination);
}