import type { SharedMockDatabase } from './sharedMock.types'
const STORAGE_KEY='productapp.shared-mock.v1'
export function readSharedMockStorage():SharedMockDatabase|null{if(typeof window==='undefined')return null;try{const value=window.localStorage.getItem(STORAGE_KEY);if(!value)return null;const parsed=JSON.parse(value) as SharedMockDatabase;return parsed.schemaVersion===1?parsed:null}catch{return null}}
export function writeSharedMockStorage(database:SharedMockDatabase):void{if(typeof window==='undefined')return;window.localStorage.setItem(STORAGE_KEY,JSON.stringify(database))}
export function clearSharedMockStorage():void{if(typeof window==='undefined')return;window.localStorage.removeItem(STORAGE_KEY)}
