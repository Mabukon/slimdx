/*
* Copyright (c) 2007-2008 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#include <d3d9.h>
#include <d3dx9.h>

#include "Direct3D9Exception.h"

#include "Device.h"
#include "Effect.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

namespace SlimDX
{
	namespace Direct3D9
	{
		EffectPool::EffectPool( IntPtr pointer )
		{
			Construct( pointer, NativeInterface );
		}

		EffectPool::EffectPool()
		{
			ID3DXEffectPool* pointer;
			HRESULT hr = D3DXCreateEffectPool( &pointer );
			if( FAILED( hr ) )
				throw gcnew Direct3D9Exception();

			Construct( pointer );
		}

		Effect::Effect( ID3DXEffect* pointer )
		{
			shim = NULL;
			Construct( pointer );
		}

		Effect::Effect( IntPtr pointer )
		{
			shim = NULL;
			Construct( pointer, NativeInterface );
		}

		Effect^ Effect::FromMemory( Device^ device, array<Byte>^ memory, array<Macro>^ preprocessorDefines,
			Include ^includeFile, String^ skipConstants, ShaderFlags flags, EffectPool^ pool,
			[Out] String^ %compilationErrors )
		{
			ID3DXEffect* effect;
			ID3DXBuffer* errorBuffer;
			pin_ptr<unsigned char> pinnedData = &memory[0];

			LPCSTR skipString = NULL;
			pin_ptr<Byte> pinnedSkip;
			if( skipConstants != nullptr )
			{
				array<Byte>^ skipBytes = System::Text::ASCIIEncoding::ASCII->GetBytes( skipConstants );
				pinnedSkip = &skipBytes[0];
				skipString = reinterpret_cast<LPCSTR>( pinnedSkip );
			}

			IncludeShim includeShim = IncludeShim( includeFile );
			ID3DXInclude* includePtr = NULL;
			if( includeFile != nullptr )
				includePtr = &includeShim;

			ID3DXEffectPool* effectPool = pool != nullptr ? pool->InternalPointer : NULL;
			array<GCHandle>^ handles;
			D3DXMACRO* macros = Macro::Marshal( preprocessorDefines, handles );

			HRESULT hr = D3DXCreateEffectEx( device->InternalPointer, pinnedData, memory->Length, macros, includePtr,
				skipString, static_cast<DWORD>( flags ), effectPool, &effect, &errorBuffer );
			
			//clean up after marshaling macros
			Macro::Unmarshal( macros, handles );
			//marshal errors if necessary
			compilationErrors = Utilities::BufferToString( errorBuffer );
			
			if( Result::Record( hr ).IsFailure )
				return nullptr;

			return gcnew Effect( effect );
		}

		Effect^ Effect::FromStream( Device^ device, Stream^ stream, array<Macro>^ preprocessorDefines, Include^ includeFile,
			String^ skipConstants, ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors )
		{
			array<Byte>^ data = Utilities::ReadStream( stream, 0 );

			return Effect::FromMemory( device, data, preprocessorDefines, includeFile,
				skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromStream( Device^ device, Stream^ stream, Include^ includeFile, String^ skipConstants,
			ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors )
		{
			return FromStream( device, stream, nullptr, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromStream( Device^ device, Stream^ stream, Include^ includeFile, 
			String^ skipConstants, ShaderFlags flags, EffectPool^ pool )
		{
			String^ compilationErrors;
			return FromStream( device, stream, nullptr, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromString( Device^ device, String^ sourceData, array<Macro>^ preprocessorDefines, Include^ includeFile,
			String^ skipConstants, ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors )
		{
			array<Byte>^ data = System::Text::ASCIIEncoding::ASCII->GetBytes( sourceData );

			return Effect::FromMemory( device, data, preprocessorDefines, includeFile,
				skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromString( Device^ device, String^ sourceData, Include^ includeFile, String^ skipConstants,
			ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors)
		{
			return FromString(device, sourceData, nullptr, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromString( Device^ device, String^ sourceData, Include^ includeFile, String^ skipConstants,
			ShaderFlags flags, EffectPool^ pool)
		{
			String^ compilationErrors;
			return FromString(device, sourceData, nullptr, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromFile( Device^ device, String^ fileName, array<Macro>^ preprocessorDefines, Include^ includeFile,
			String^ skipConstants, ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors )
		{
			ID3DXEffect* effect;
			ID3DXBuffer* errorBuffer;
			pin_ptr<const wchar_t> pinnedName = PtrToStringChars( fileName );

			LPCSTR skipString = NULL;
			pin_ptr<Byte> pinnedSkip;
			if( skipConstants != nullptr )
			{
				array<Byte>^ skipBytes = System::Text::ASCIIEncoding::ASCII->GetBytes( skipConstants );
				pinnedSkip = &skipBytes[0];
				skipString = reinterpret_cast<LPCSTR>( pinnedSkip );
			}

			IncludeShim includeShim = IncludeShim( includeFile );
			ID3DXInclude* includePtr = NULL;
			if( includeFile != nullptr )
				includePtr = &includeShim;

			ID3DXEffectPool* effectPool = pool != nullptr ? pool->InternalPointer : NULL;
			array<GCHandle>^ handles;
			D3DXMACRO* macros = Macro::Marshal( preprocessorDefines, handles );

			HRESULT hr = D3DXCreateEffectFromFileEx( device->InternalPointer, pinnedName, macros, includePtr,
				skipString, static_cast<DWORD>( flags ), effectPool, &effect, &errorBuffer );
			
			//clean up after marshaling macros
			Macro::Unmarshal( macros, handles );
			//marshal errors if necessary
			compilationErrors = Utilities::BufferToString( errorBuffer );
			
			if( Result::Record( hr ).IsFailure )
				return nullptr;

			if( effect == NULL )
				return nullptr;

			return gcnew Effect( effect );
		}

		Effect^ Effect::FromFile( Device^ device, String^ fileName, Include^ includeFile, String^ skipConstants,
			ShaderFlags flags, EffectPool^ pool, [Out] String^ %compilationErrors)
		{
			return Effect::FromFile( device, fileName, nullptr, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		Effect^ Effect::FromFile( Device^ device, String^ fileName, Include^ includeFile, String^ skipConstants,
			ShaderFlags flags, EffectPool^ pool)
		{
			String^ compilationErrors;
			return Effect::FromFile( device, fileName, includeFile, skipConstants, flags, pool, compilationErrors );
		}

		int Effect::Begin( FX flags )
		{
			unsigned int passCount;

			HRESULT hr = EffectPointer->Begin( &passCount, static_cast<DWORD>( flags ) );
			Result::Record( hr );

			return passCount;
		}

		Result Effect::End()
		{
			HRESULT hr = EffectPointer->End();
			return Result::Record( hr );
		}

		Result Effect::BeginPass( int pass )
		{
			HRESULT hr = EffectPointer->BeginPass( pass );
			return Result::Record( hr );
		}

		Result Effect::EndPass()
		{
			HRESULT hr = EffectPointer->EndPass();
			return Result::Record( hr );
		}

		Result Effect::BeginParameterBlock()
		{
			HRESULT hr = EffectPointer->BeginParameterBlock();
			return Result::Record( hr );
		}

		EffectHandle^ Effect::EndParameterBlock()
		{
			D3DXHANDLE handle = EffectPointer->EndParameterBlock();
			if( handle == NULL )
				return nullptr;
			return gcnew EffectHandle( handle );
		}

		Result Effect::ApplyParameterBlock( EffectHandle^ parameterBlock )
		{
			D3DXHANDLE handle = parameterBlock != nullptr ? parameterBlock->InternalHandle : NULL;
			HRESULT hr = EffectPointer->ApplyParameterBlock( handle );
			return Result::Record( hr );
		}

		Result Effect::DeleteParameterBlock( EffectHandle^ parameterBlock )
		{
			D3DXHANDLE handle = parameterBlock != nullptr ? parameterBlock->InternalHandle : NULL;
			HRESULT hr = EffectPointer->DeleteParameterBlock( handle );
			return Result::Record( hr );
		}

		bool Effect::IsParameterUsed( EffectHandle^ parameter, EffectHandle^ technique )
		{
			D3DXHANDLE paramHandle = parameter != nullptr ? parameter->InternalHandle : NULL;
			D3DXHANDLE techHandle = technique != nullptr ? technique->InternalHandle : NULL;
			BOOL used = EffectPointer->IsParameterUsed( paramHandle, techHandle );
			return used > 0;
		}

		Result Effect::CommitChanges()
		{
			HRESULT hr = EffectPointer->CommitChanges();
			return Result::Record( hr );
		}

		EffectHandle^ Effect::FindNextValidTechnique( EffectHandle^ technique )
		{
			D3DXHANDLE handle;
			D3DXHANDLE parentHandle = technique != nullptr ? technique->InternalHandle : NULL;

			HRESULT hr = EffectPointer->FindNextValidTechnique( parentHandle, &handle );

			if( Result::Record( hr ).IsFailure || handle == NULL )
				return nullptr;

			return gcnew EffectHandle( handle );
		}

		bool Effect::ValidateTechnique( EffectHandle^ technique )
		{
			D3DXHANDLE handle = technique != nullptr ? technique->InternalHandle : NULL;
			return FAILED( EffectPointer->ValidateTechnique( handle ) );
		}

		EffectHandle^ Effect::Technique::get()
		{
			D3DXHANDLE handle = EffectPointer->GetCurrentTechnique();
			if( handle == NULL )
				return nullptr;
			return gcnew EffectHandle( handle );
		}

		void Effect::Technique::set( EffectHandle^ value )
		{
			D3DXHANDLE handle = value != nullptr ? value->InternalHandle : NULL;
			HRESULT hr = EffectPointer->SetTechnique( handle );
			Result::Record( hr );
		}

		Result Effect::OnLostDevice()
		{
			HRESULT hr = EffectPointer->OnLostDevice();
			return Result::Record( hr );
		}

		Result Effect::OnResetDevice()
		{
			HRESULT hr = EffectPointer->OnResetDevice();
			return Result::Record( hr );
		}

		Result Effect::SetStateManager( IEffectStateManager^ manager )
		{
			SLIMDX_DEBUG_UNREFERENCED_PARAMETER(manager);

			if( shim != NULL )
				delete shim;

			//shim = new IEffectStateManagerShim( manager );

			HRESULT hr = EffectPointer->SetStateManager( shim );
			return Result::Record( hr );
		}

		IEffectStateManager^ Effect::GetStateManager()
		{
			if( shim != NULL )
				return shim->GetManager();
			else
				return nullptr;
		}
	}
}
