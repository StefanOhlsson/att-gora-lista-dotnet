export type Priority = 'LAG' | 'NORMAL' | 'HOG' | 'KRITISK';
export type TodoStatus = 'REGISTRERAD' | 'STARTAD' | 'AVSLUTAD';
export interface History { id:number; fromStatus:TodoStatus; toStatus:TodoStatus; changedAt:string }
export interface Todo { id:number; title:string; description:string; priority:Priority; status:TodoStatus; createdAt:string; updatedAt:string; history:History[] }
export interface NewTodo { title:string; description:string; priority:Priority; status:TodoStatus }
