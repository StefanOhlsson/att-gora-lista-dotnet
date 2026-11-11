import axios from 'axios';
import type { NewTodo, Todo, TodoStatus } from './types';

// Adressen kan ändras vid build med VITE_API_BASE_URL.
const api = axios.create({ baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080/api' });
export const getTodos = async () => (await api.get<Todo[]>('/todos')).data;
export const createTodo = async (todo:NewTodo) => (await api.post<Todo>('/todos', todo)).data;
export const changeStatus = async (id:number, status:TodoStatus) => (await api.patch<Todo>(`/todos/${id}/status`, { status })).data;
export const deleteTodo = async (id:number) => { await api.delete(`/todos/${id}`); };
