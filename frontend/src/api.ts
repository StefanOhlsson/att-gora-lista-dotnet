import axios from 'axios';
import type { NewTodo, Todo, TodoStatus } from './types';

// Alla anrop till backend samlas här så att komponenterna inte behöver känna till URL:er.
// Axios kastar ett fel för svar med statuskod 4xx/5xx; det fångas i App.tsx (run).

// Adressen kan ändras vid build med VITE_API_BASE_URL.
const api = axios.create({ baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080/api' });

// GET /api/todos – hämtar alla uppgifter inklusive statushistorik.
export const getTodos = async () => (await api.get<Todo[]>('/todos')).data;

// POST /api/todos – skapar en ny uppgift. Backend svarar med den sparade uppgiften (med id).
export const createTodo = async (todo:NewTodo) => (await api.post<Todo>('/todos', todo)).data;

// PATCH /api/todos/{id}/status – byter status. Backend loggar bytet i historiken.
export const changeStatus = async (id:number, status:TodoStatus) => (await api.patch<Todo>(`/todos/${id}/status`, { status })).data;

// DELETE /api/todos/{id} – tar bort uppgiften (och dess historik). Svaret har inget innehåll.
export const deleteTodo = async (id:number) => { await api.delete(`/todos/${id}`); };
